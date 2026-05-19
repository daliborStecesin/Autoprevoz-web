using Transport.Web.Components;
using Transport.Infrastructure.Data;
using Transport.Application.Interfaces;
using Transport.Application.Services;
using Transport.Web.Services;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// DATABASE — Transport baza (kasa)
// ============================================================================
// TransportDbContext koristi connection string klijentove baze iz tenant cookie-ja
builder.Services.AddScoped<TransportDbContext>(sp =>
{
    var tenant = sp.GetRequiredService<ITenantService>();
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
    return new TransportDbContext(opts);
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

// ============================================================================
// INFRASTRUCTURE
// ============================================================================
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<INbsPartnerService, NbsPartnerService>();
builder.Services.AddHttpContextAccessor();

// ============================================================================
// TENANT — čita auth iz HTTP cookie-ja (dostupno u SSR i interactive)
// ============================================================================
builder.Services.AddScoped<ITenantService, TenantService>();

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
        .FirstOrDefaultAsync(k => k.Email       == podaci.Email
                                && k.LozinkaHash == podaci.Password
                                && k.Aktivan     == 1);

    if (korisnik is null)
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
        SameSite  = SameSiteMode.Lax,
        Expires   = DateTimeOffset.UtcNow.AddHours(8)
    };

    ctx.Response.Cookies.Append("ap_conn",  licenca.ConnectionString ?? string.Empty, opts);
    ctx.Response.Cookies.Append("ap_firma", licenca.Naziv            ?? string.Empty, opts);
    ctx.Response.Cookies.Append("ap_priv",  korisnik.Privilegija.ToString(),          opts);

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
    ctx.Response.Cookies.Delete("ap_conn");
    ctx.Response.Cookies.Delete("ap_firma");
    ctx.Response.Cookies.Delete("ap_priv");
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginPodaci(string Email, string Password);
