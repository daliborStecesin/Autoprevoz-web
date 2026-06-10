using Transport.Web.Components;
using Transport.Infrastructure;
using Transport.Infrastructure.Data;
using Transport.Application.Interfaces;
using Transport.Application.Services;
using Transport.Application.Services.Sef;
using Transport.Web.Services;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// DATABASE — Transport baza (kasa)
// ============================================================================
// TransportDbContext koristi connection string klijentove baze iz tenant cookie-ja
builder.Services.AddScoped<TransportDbContext>(sp =>
{
    var tenant      = sp.GetRequiredService<ITenantService>();
    var currentUser = sp.GetRequiredService<ICurrentUser>();
    var config  = sp.GetRequiredService<IConfiguration>();
    var raw = tenant.IsAuthenticated()
        ? tenant.GetConnectionString()
        : config.GetConnectionString("Transport") ?? string.Empty;

    // Osiguramo TrustServerCertificate bez obzira šta je u licence connection stringu
    var csb = new SqlConnectionStringBuilder(raw)
    {
        TrustServerCertificate = true,
        Encrypt = false
    };

    var opts = new DbContextOptionsBuilder<TransportDbContext>()
        .UseSqlServer(csb.ConnectionString)
        .Options;
    return new TransportDbContext(opts, currentUser);
});

// ============================================================================
// DATABASE — Master baza (daksoft) — licence i web korisnici
// ============================================================================
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Master")));

// ============================================================================
// APPLICATION SERVICES
// ============================================================================
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IRacunService, RacunService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IDnevnicaService, DnevnicaService>();
builder.Services.AddScoped<IPdvService, PdvService>();
builder.Services.AddScoped<IDefaultValuesService, DefaultValuesService>();
builder.Services.AddScoped<SefApiClient>();
builder.Services.AddScoped<ISefService, SefService>();

// ============================================================================
// INFRASTRUCTURE
// ============================================================================
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<INbsPartnerService, NbsPartnerService>();
builder.Services.AddSingleton<INbsKursService, NbsKursService>();
builder.Services.AddHttpContextAccessor();

// ============================================================================
// TENANT — čita auth iz HTTP cookie-ja (dostupno u SSR i interactive)
// ============================================================================
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

// ============================================================================
// UI COMPONENTS — MudBlazor
// ============================================================================
builder.Services.AddMudServices();

// ============================================================================
// RAZOR COMPONENTS & BLAZOR
// ============================================================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ============================================================================
// API — Prijava: validira kredencijale i postavlja HTTP cookie
// ============================================================================
app.MapPost("/api/auth/login", async (LoginPodaci podaci, MasterDbContext db, HttpContext ctx) =>
{
    var korisnik = await db.WebKorisnici
        .Include(k => k.Licenca)
        .FirstOrDefaultAsync(k => k.Email   == podaci.Email
                                && k.Aktivan == 1);

    if (korisnik is null)
        return Results.Json(new { poruka = "Pogrešan email ili lozinka." }, statusCode: 401);

    var hasher = new PasswordHasher<object>();
    var verResult = hasher.VerifyHashedPassword(new object(), korisnik.LozinkaHash ?? "", podaci.Password);
    if (verResult == PasswordVerificationResult.Failed)
        return Results.Json(new { poruka = "Pogrešan email ili lozinka." }, statusCode: 401);

    var licenca = korisnik.Licenca;

    if (licenca is null || licenca.WebAktivan != 1)
        return Results.Json(new { poruka = "Nemate aktiviran web pristup." }, statusCode: 401);

    if (licenca.DatumLicence.HasValue &&
        licenca.DatumLicence.Value < DateOnly.FromDateTime(DateTime.Today))
        return Results.Json(
            new { poruka = $"Licenca je istekla {licenca.DatumLicence.Value:dd.MM.yyyy}. Kontaktirajte podršku." },
            statusCode: 401);

    var opts = new CookieOptions
    {
        HttpOnly  = true,
        Secure    = false,
        SameSite  = SameSiteMode.Lax,
        Path      = "/",
        Expires   = DateTimeOffset.UtcNow.AddHours(8)
    };

    korisnik.ZadnjaPrijava = DateTime.Now;
    await db.SaveChangesAsync();

    ctx.Response.Cookies.Append("ap_firma",   licenca.Naziv                ?? string.Empty, opts);
    ctx.Response.Cookies.Append("ap_ime",     korisnik.Ime                 ?? string.Empty, opts);
    ctx.Response.Cookies.Append("ap_priv",    korisnik.Privilegija.ToString(),              opts);
    ctx.Response.Cookies.Append("ap_user",    korisnik.IdKorisnika.ToString(),              opts);
    ctx.Response.Cookies.Append("ap_licence", korisnik.IdLicence.ToString(),               opts);

    // Učitaj module zastavice iz tenant baze (raw ADO.NET — ne zavisi od EF mapiranja)
    // NULL u bazi = 0 (neaktivan); greška čitanja = 1 (backwards compatible)
    int transportAktivan = 1;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(licenca.ConnectionString ?? string.Empty)
        {
            TrustServerCertificate = true,
            Encrypt = false
        };
        using var conn = new SqlConnection(tenantCsb.ConnectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 transportModulAktivan FROM tbl_Podesavanja WHERE Broj = 1";
        var val = await cmd.ExecuteScalarAsync();
        // NULL ili DBNull → 0 (neaktivan); sve ostalo → int vrednost
        transportAktivan = (val is DBNull or null) ? 0 : Convert.ToInt32(val);
    }
    catch
    {
        // Kolona ne postoji (ALTER nije pokrenut) → legacy ponašanje: aktivan
        transportAktivan = 1;
    }

    ctx.Response.Cookies.Append("ap_transport", transportAktivan.ToString(), opts);

    return Results.Ok(new
    {
        connectionString = licenca.ConnectionString ?? string.Empty,
        nazivFirme       = licenca.Naziv            ?? string.Empty,
        idKorisnika      = korisnik.IdKorisnika,
        privilegija      = korisnik.Privilegija
    });
});

// ============================================================================
// API — Odjava: briše cookie i vraća na /login
// ============================================================================
app.MapGet("/api/auth/logout", (HttpContext ctx) =>
{
    var deleteOpts = new CookieOptions { Path = "/" };
    ctx.Response.Cookies.Delete("ap_conn",      deleteOpts);
    ctx.Response.Cookies.Delete("ap_firma",     deleteOpts);
    ctx.Response.Cookies.Delete("ap_ime",       deleteOpts);
    ctx.Response.Cookies.Delete("ap_priv",      deleteOpts);
    ctx.Response.Cookies.Delete("ap_user",      deleteOpts);
    ctx.Response.Cookies.Delete("ap_transport", deleteOpts);
    ctx.Response.Cookies.Delete("ap_licence",   deleteOpts);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginPodaci(string Email, string Password);
