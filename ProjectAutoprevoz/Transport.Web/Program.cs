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
string ResolveTenantConnectionString(IServiceProvider sp)
{
    var tenant = sp.GetRequiredService<ITenantService>();
    var config = sp.GetRequiredService<IConfiguration>();
    var raw = tenant.IsAuthenticated()
        ? tenant.GetConnectionString()
        : config.GetConnectionString("Transport") ?? string.Empty;

    // Osiguramo TrustServerCertificate bez obzira šta je u licence connection stringu
    var csb = new SqlConnectionStringBuilder(raw)
    {
        TrustServerCertificate = true,
        Encrypt = false
    };
    return csb.ConnectionString;
}

builder.Services.AddScoped<TransportDbContext>(sp =>
{
    var currentUser = sp.GetRequiredService<ICurrentUser>();
    var opts = new DbContextOptionsBuilder<TransportDbContext>()
        .UseSqlServer(ResolveTenantConnectionString(sp))
        .Options;
    return new TransportDbContext(opts, currentUser);
});

// IDbContextFactory — za stranice koje moraju da otvaraju kratkotrajne, izolovane
// kontekste po operaciji (izbegava "A second operation was started on this context
// instance before a previous operation completed" kod preklapajućih async poziva
// u istom Blazor circuit-u). Scoped da bi tenant/connection string i dalje bili
// po korisniku/krugu.
builder.Services.AddDbContextFactory<TransportDbContext>((sp, options) =>
{
    options.UseSqlServer(ResolveTenantConnectionString(sp));
}, ServiceLifetime.Scoped);

// ============================================================================
// DATABASE — Master baza (daksoft) — licence i web korisnici
// ============================================================================
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Master")));

// IDbContextFactory — isti razlog kao za TransportDbContext iznad: stranice/layouti
// koji moraju da otvore kratkotrajan, izolovan kontekst po operaciji (SuperAdminLayout
// guard + /ds stranica inicijalizuju se preklapajuće u istom Blazor circuit-u, pa bi
// deljeni scoped MasterDbContext izazvao "A second operation was started...").
// MORA biti ServiceLifetime.Scoped (ne default Singleton) — DbContextOptions<MasterDbContext>
// je već registrovan kao Scoped preko AddDbContext iznad, a Singleton ne sme da zavisi
// od Scoped servisa (isti razlog kao TransportDbContext factory ispod).
builder.Services.AddDbContextFactory<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Master")), ServiceLifetime.Scoped);

// ============================================================================
// APPLICATION SERVICES
// ============================================================================
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IDnevnicaService, DnevnicaService>();
builder.Services.AddScoped<IPdvService, PdvService>();
builder.Services.AddScoped<IDefaultValuesService, DefaultValuesService>();
builder.Services.AddScoped<IKursService, KursService>();
builder.Services.AddScoped<IKarticaService, KarticaService>();
builder.Services.AddScoped<IKarticaNovaService, KarticaNovaService>();
builder.Services.AddScoped<ILogBrisanjaService, LogBrisanjaService>();
builder.Services.AddScoped<SefApiClient>();
builder.Services.AddScoped<ISefService, SefService>();
builder.Services.AddScoped<IObavestenjaPPService, ObavestenjaPPService>();
builder.Services.AddScoped<IPojedinacnaEvidencijaPDVService, PojedinacnaEvidencijaPDVService>();
builder.Services.AddScoped<IZbirnaEvidencijaPDVService, ZbirnaEvidencijaPDVService>();
builder.Services.AddScoped<IEFaktureUlazService, EFaktureUlazService>();
builder.Services.AddScoped<IEFaktureIzlazService, EFaktureIzlazService>();
builder.Services.AddSingleton<IEFakturaUblBuilder, EFakturaUblBuilder>();
builder.Services.AddSingleton<IGreskaEfakturaPrevodService, GreskaEfakturaPrevodService>();

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
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

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
// API — Prijava: zajednička logika za JSON (fetch) i form (POST) login
// ============================================================================
async Task<LoginRezultatInterno> PrijaviKorisnikaAsync(string email, string password, MasterDbContext db, HttpContext ctx)
{
    var korisnik = await db.WebKorisnici
        .Include(k => k.Licenca)
        .FirstOrDefaultAsync(k => k.Email   == email
                                && k.Aktivan == 1);

    if (korisnik is null)
        return new LoginRezultatInterno(false, "Pogrešan email ili lozinka.", null, null, 0, 0);

    var hasher = new PasswordHasher<object>();
    var verResult = hasher.VerifyHashedPassword(new object(), korisnik.LozinkaHash ?? "", password);
    if (verResult == PasswordVerificationResult.Failed)
        return new LoginRezultatInterno(false, "Pogrešan email ili lozinka.", null, null, 0, 0);

    var optsSuperAdmin = new CookieOptions
    {
        HttpOnly  = true,
        Secure    = false,
        SameSite  = SameSiteMode.Lax,
        Path      = "/",
        Expires   = DateTimeOffset.UtcNow.AddHours(8)
    };

    // Superadmin (Privilegija=9) nema IdLicence/tenant bazu — preskače proveru
    // licence i oba raw ADO upita na tenant tbl_Podesavanja ispod. Dobija SAMO
    // ap_user/ap_ime/ap_priv, bez ap_licence/ap_firma/ap_transport/ap_efaktura.
    if (korisnik.Privilegija == 9)
    {
        korisnik.ZadnjaPrijava = DateTime.Now;
        await db.SaveChangesAsync();

        ctx.Response.Cookies.Append("ap_user", korisnik.IdKorisnika.ToString(), optsSuperAdmin);
        ctx.Response.Cookies.Append("ap_ime",  korisnik.Ime ?? string.Empty,    optsSuperAdmin);
        ctx.Response.Cookies.Append("ap_priv", korisnik.Privilegija.ToString(), optsSuperAdmin);

        return new LoginRezultatInterno(true, null, null, null, korisnik.IdKorisnika, korisnik.Privilegija);
    }

    var licenca = korisnik.Licenca;

    if (licenca is null || licenca.WebAktivan != 1)
        return new LoginRezultatInterno(false, "Nemate aktiviran web pristup.", null, null, 0, 0);

    if (licenca.DatumLicence.HasValue &&
        licenca.DatumLicence.Value < DateOnly.FromDateTime(DateTime.Today))
        return new LoginRezultatInterno(false, $"Licenca je istekla {licenca.DatumLicence.Value:dd.MM.yyyy}. Kontaktirajte podršku.", null, null, 0, 0);

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

    // Učitaj zastavicu e-faktura modula (OpcijaInt13) iz tenant baze
    // NULL u bazi = 0 (neaktivan); greška čitanja = 0 (novo polje, konzervativno)
    int eFakturaAktivna = 0;
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
        cmd.CommandText = "SELECT TOP 1 OpcijaInt13 FROM tbl_Podesavanja WHERE Broj = 1";
        var val = await cmd.ExecuteScalarAsync();
        eFakturaAktivna = (val is DBNull or null) ? 0 : Convert.ToInt32(val);
    }
    catch
    {
        eFakturaAktivna = 0;
    }

    ctx.Response.Cookies.Append("ap_efaktura", eFakturaAktivna.ToString(), opts);

    return new LoginRezultatInterno(true, null, licenca.ConnectionString ?? string.Empty, licenca.Naziv ?? string.Empty, korisnik.IdKorisnika, korisnik.Privilegija);
}

// API — Prijava (JSON/fetch) — zadržano za kompatibilnost
app.MapPost("/api/auth/login", async (LoginPodaci podaci, MasterDbContext db, HttpContext ctx) =>
{
    var rez = await PrijaviKorisnikaAsync(podaci.Email, podaci.Password, db, ctx);

    if (!rez.Success)
        return Results.Json(new { poruka = rez.Poruka }, statusCode: 401);

    return Results.Ok(new
    {
        connectionString = rez.ConnectionString,
        nazivFirme       = rez.NazivFirme,
        idKorisnika      = rez.IdKorisnika,
        privilegija      = rez.Privilegija
    });
});

// API — Prijava (klasičan <form method="post">, puna navigacija — radi pouzdano i na mobilnim browserima)
app.MapPost("/api/auth/login-form", async (HttpContext ctx, MasterDbContext db) =>
{
    var form     = await ctx.Request.ReadFormAsync();
    var email    = form["email"].ToString();
    var password = form["password"].ToString();

    var rez = await PrijaviKorisnikaAsync(email, password, db, ctx);

    if (!rez.Success)
        return Results.Redirect($"/login?greska={Uri.EscapeDataString(rez.Poruka ?? "Greška pri prijavljivanju.")}");

    return Results.Redirect(rez.Privilegija == 9 ? "/ds" : "/dashboard");
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
    ctx.Response.Cookies.Delete("ap_efaktura",  deleteOpts);
    ctx.Response.Cookies.Delete("ap_licence",   deleteOpts);
    return Results.Redirect("/login");
});

// ============================================================================
// API — Superadmin: uđi u firmu (impersonacija) / vrati se u panel
// ============================================================================
// Kolačići se ne mogu postaviti iz Blazor interaktivne komponente (odgovor je već
// poslat), zato sve ide preko HTTP endpointa + forceLoad navigacije (isti razlog
// kao kod /api/auth/login-form).
app.MapGet("/api/superadmin/udji", async (int id, HttpContext ctx, MasterDbContext db) =>
{
    // SIGURNOSNA PROVERA — ne veruj kolačiću ap_priv, proveri stvarno stanje u master bazi.
    var idKorisnika = int.TryParse(ctx.Request.Cookies["ap_user"], out var uid) ? uid : 0;

    var korisnik = await db.WebKorisnici.AsNoTracking()
        .FirstOrDefaultAsync(k => k.IdKorisnika == idKorisnika);

    if (korisnik is null || korisnik.Aktivan != 1 || korisnik.Privilegija != 9)
        return Results.Redirect("/login");

    var licenca = await db.Licence.AsNoTracking()
        .FirstOrDefaultAsync(l => l.IdLicence == id);

    if (licenca is null || string.IsNullOrEmpty(licenca.ConnectionString))
        return Results.Redirect("/ds?greska=" + Uri.EscapeDataString("Licenca nema connection string"));

    var opts = new CookieOptions
    {
        HttpOnly  = true,
        Secure    = false,
        SameSite  = SameSiteMode.Lax,
        Path      = "/",
        Expires   = DateTimeOffset.UtcNow.AddHours(8)
    };

    ctx.Response.Cookies.Append("ap_licence",     id.ToString(),                 opts);
    ctx.Response.Cookies.Append("ap_firma",       licenca.Naziv ?? string.Empty, opts);
    ctx.Response.Cookies.Append("ap_priv",        "1",                           opts); // ap_user/ap_ime se NE diraju — aplikacija se ponaša kao normalna prijava
    ctx.Response.Cookies.Append("ap_impersonate", "1",                           opts);

    // Isti raw ADO upiti na tenant tbl_Podesavanja kao u PrijaviKorisnikaAsync.
    // Ako upit pukne, oba idu na "0" — ulazak u firmu se ne ruši zbog toga.
    int transportAktivan = 0;
    int eFakturaAktivna  = 0;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(licenca.ConnectionString ?? string.Empty)
        {
            TrustServerCertificate = true,
            Encrypt = false
        };
        using var conn = new SqlConnection(tenantCsb.ConnectionString);
        await conn.OpenAsync();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT TOP 1 transportModulAktivan FROM tbl_Podesavanja WHERE Broj = 1";
            var val = await cmd.ExecuteScalarAsync();
            transportAktivan = (val is DBNull or null) ? 0 : Convert.ToInt32(val);
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT TOP 1 OpcijaInt13 FROM tbl_Podesavanja WHERE Broj = 1";
            var val = await cmd.ExecuteScalarAsync();
            eFakturaAktivna = (val is DBNull or null) ? 0 : Convert.ToInt32(val);
        }
    }
    catch
    {
        transportAktivan = 0;
        eFakturaAktivna  = 0;
    }

    ctx.Response.Cookies.Append("ap_transport", transportAktivan.ToString(), opts);
    ctx.Response.Cookies.Append("ap_efaktura",  eFakturaAktivna.ToString(),  opts);

    return Results.Redirect("/dashboard");
});

app.MapGet("/api/superadmin/izadji", (HttpContext ctx) =>
{
    var deleteOpts = new CookieOptions { Path = "/" };
    ctx.Response.Cookies.Delete("ap_licence",     deleteOpts);
    ctx.Response.Cookies.Delete("ap_firma",       deleteOpts);
    ctx.Response.Cookies.Delete("ap_impersonate", deleteOpts);
    ctx.Response.Cookies.Delete("ap_transport",   deleteOpts);
    ctx.Response.Cookies.Delete("ap_efaktura",    deleteOpts);

    var opts = new CookieOptions
    {
        HttpOnly  = true,
        Secure    = false,
        SameSite  = SameSiteMode.Lax,
        Path      = "/",
        Expires   = DateTimeOffset.UtcNow.AddHours(8)
    };
    ctx.Response.Cookies.Append("ap_priv", "9", opts);

    return Results.Redirect("/ds");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginPodaci(string Email, string Password);

record LoginRezultatInterno(bool Success, string? Poruka, string? ConnectionString, string? NazivFirme, int IdKorisnika, int Privilegija);
