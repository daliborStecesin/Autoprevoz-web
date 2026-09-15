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
using Microsoft.AspNetCore.DataProtection;

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
        .UseSqlServer(ResolveTenantConnectionString(sp), sql => sql.UseCompatibilityLevel(120))
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
    options.UseSqlServer(ResolveTenantConnectionString(sp), sql => sql.UseCompatibilityLevel(120));
}, ServiceLifetime.Scoped);

// ============================================================================
// DATABASE — Master baza (daksoft) — licence i web korisnici
// ============================================================================
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Master"), sql => sql.UseCompatibilityLevel(120)));

// IDbContextFactory — isti razlog kao za TransportDbContext iznad: stranice/layouti
// koji moraju da otvore kratkotrajan, izolovan kontekst po operaciji (SuperAdminLayout
// guard + /ds stranica inicijalizuju se preklapajuće u istom Blazor circuit-u, pa bi
// deljeni scoped MasterDbContext izazvao "A second operation was started...").
// MORA biti ServiceLifetime.Scoped (ne default Singleton) — DbContextOptions<MasterDbContext>
// je već registrovan kao Scoped preko AddDbContext iznad, a Singleton ne sme da zavisi
// od Scoped servisa (isti razlog kao TransportDbContext factory ispod).
builder.Services.AddDbContextFactory<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Master"), sql => sql.UseCompatibilityLevel(120)), ServiceLifetime.Scoped);

// ============================================================================
// APPLICATION SERVICES
// ============================================================================
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IDnevnicaService, DnevnicaService>();
builder.Services.AddScoped<IDefaultValuesService, DefaultValuesService>();
builder.Services.AddScoped<IKursService, KursService>();
builder.Services.AddScoped<IKarticaService, KarticaService>();
builder.Services.AddScoped<IKarticaNovaService, KarticaNovaService>();
builder.Services.AddScoped<ILogBrisanjaService, LogBrisanjaService>();
builder.Services.AddScoped<IModulService, ModulService>();
builder.Services.AddScoped<IProfilService, ProfilService>();
builder.Services.AddScoped<SefApiClient>();
builder.Services.AddScoped<ISefService, SefService>();
builder.Services.AddScoped<IObavestenjaPPService, ObavestenjaPPService>();
builder.Services.AddScoped<IPojedinacnaEvidencijaPDVService, PojedinacnaEvidencijaPDVService>();
builder.Services.AddScoped<IZbirnaEvidencijaPDVService, ZbirnaEvidencijaPDVService>();
builder.Services.AddScoped<IEFaktureUlazService, EFaktureUlazService>();
builder.Services.AddScoped<IEFaktureIzlazService, EFaktureIzlazService>();
builder.Services.AddSingleton<IEFakturaUblBuilder, EFakturaUblBuilder>();
builder.Services.AddSingleton<IGreskaEfakturaPrevodService, GreskaEfakturaPrevodService>();

// ProvisioningService čita 01_CREATE_kasa_template.sql kao embedded resurs iz OVOG
// sklopa (Transport.Web) — servis živi u Transport.Application (bez reference nazad
// na Web), pa se Assembly prosleđuje eksplicitno kroz konstruktor umesto reflection-a
// unutar servisa.
builder.Services.AddScoped<IProvisioningService>(sp => new ProvisioningService(
    sp.GetRequiredService<IDbContextFactory<MasterDbContext>>(),
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<ILogger<ProvisioningService>>(),
    typeof(Program).Assembly));

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
// DATA PROTECTION — potpisani/enkriptovani kolačići (KolacicService)
// ============================================================================
// Bez PersistKeysToFileSystem, ključevi žive samo u memoriji procesa — svaki
// restart/redeploy bi odjavio SVE korisnike (Unprotect ranije potpisanih
// kolačića bi počeo da baca/vraća null). Putanja mora biti VAN foldera
// aplikacije (redeploy ga briše/zamenjuje) i upisiva za korisnika pod kojim
// aplikacija radi (na Linux test serveru).
var putanjaKljuceva = builder.Configuration["DataProtection:PutanjaKljuceva"];
var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("Autoprevoz");
if (!string.IsNullOrWhiteSpace(putanjaKljuceva))
{
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(putanjaKljuceva));
}

builder.Services.AddSingleton<KolacicService>();

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

if (string.IsNullOrWhiteSpace(putanjaKljuceva))
{
    app.Logger.LogWarning(
        "DataProtection:PutanjaKljuceva nije podešeno — ključevi za potpisivanje kolačića " +
        "se ne čuvaju trajno na disku. Svi korisnici će biti odjavljeni pri svakom restartu " +
        "ili redeploy-u aplikacije.");
}

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
async Task<LoginRezultatInterno> PrijaviKorisnikaAsync(string email, string password, MasterDbContext db, HttpContext ctx, KolacicService kolacici)
{
    var korisnik = await db.WebKorisnici
        .FirstOrDefaultAsync(k => k.Email   == email
                                && k.Aktivan == 1);

    if (korisnik is null)
        return new LoginRezultatInterno(false, "Pogrešan email ili lozinka.", null, null, 0, 0);

    var hasher = new PasswordHasher<object>();
    var verResult = hasher.VerifyHashedPassword(new object(), korisnik.LozinkaHash ?? "", password);
    if (verResult == PasswordVerificationResult.Failed)
        return new LoginRezultatInterno(false, "Pogrešan email ili lozinka.", null, null, 0, 0);

    // Prijava je uvek nova sesija — nikad ne sme naslediti impersonaciju iz
    // prethodne sesije na istom browseru (i za običnu i za superadmin granu).
    kolacici.Obrisi(ctx, "ap_impersonate");
    // ap_licence je ukinut (v214, članstva su jedini izvor) — briše se ovde da se očiste
    // stari pretraživači koji ga još nose iz prethodne sesije.
    kolacici.Obrisi(ctx, "ap_licence");

    // Superadmin (Privilegija=9) nema tenant bazu — preskače proveru članstva
    // i oba raw ADO upita na tenant tbl_Podesavanja ispod. Dobija SAMO
    // ap_user/ap_ime/ap_priv — ap_idfirme/ap_firma/ap_transport/ap_efaktura se
    // EKSPLICITNO brišu (ne samo "ne postavljaju"), da fresh prijava nikad ne
    // nasledi tenant kontekst iz prethodne sesije na istom browseru (isti razlog
    // kao ap_impersonate/ap_licence iznad) — dok superadmin ne uđe u firmu preko
    // /udji ili /mojafirma (koji ove kolačiće postavljaju iznova, zaštićeno).
    if (korisnik.Privilegija == 9)
    {
        korisnik.ZadnjaPrijava = DateTime.Now;
        await db.SaveChangesAsync();

        kolacici.Obrisi(ctx, "ap_idfirme");
        kolacici.Obrisi(ctx, "ap_firma");
        kolacici.Obrisi(ctx, "ap_transport");
        kolacici.Obrisi(ctx, "ap_efaktura");

        kolacici.Upisi(ctx, "ap_user", korisnik.IdKorisnika.ToString());
        kolacici.Upisi(ctx, "ap_ime",  korisnik.Ime ?? string.Empty);
        kolacici.Upisi(ctx, "ap_priv", korisnik.Privilegija.ToString());

        return new LoginRezultatInterno(true, null, null, null, korisnik.IdKorisnika, korisnik.Privilegija);
    }

    // Aktivna članstva korisnika (tbl_web_clanstvo → tbl_web_licence). Zamenjuje staru
    // proveru preko tbl_web_korisnici.IdLicence → tbl_licence (v214).
    var clanstva = await db.WebClanstva
        .Include(c => c.Licenca)
        .Where(c => c.IdKorisnika == korisnik.IdKorisnika && c.Aktivan && c.Licenca!.Aktivna)
        .ToListAsync();

    if (clanstva.Count == 0)
        return new LoginRezultatInterno(false, "Vaš nalog nije povezan ni sa jednom firmom. Kontaktirajte DAK-SOFT.", null, null, 0, 0);

    // TODO: ekran izbora firme kad korisnik ima više aktivnih članstava (prompt 3) — za sad uzima prvo
    var clanstvo   = clanstva[0];
    var webLicenca = clanstvo.Licenca!;

    if (webLicenca.DatumDo.HasValue && webLicenca.DatumDo.Value < DateTime.Today)
        return new LoginRezultatInterno(false, $"Licenca je istekla {webLicenca.DatumDo.Value:dd.MM.yyyy}. Kontaktirajte podršku.", null, null, 0, 0);

    korisnik.ZadnjaPrijava = DateTime.Now;
    await db.SaveChangesAsync();

    kolacici.Upisi(ctx, "ap_firma",   webLicenca.Naziv             ?? string.Empty);
    kolacici.Upisi(ctx, "ap_ime",     korisnik.Ime                 ?? string.Empty);
    kolacici.Upisi(ctx, "ap_priv",    korisnik.Privilegija.ToString());
    kolacici.Upisi(ctx, "ap_user",    korisnik.IdKorisnika.ToString());
    // ap_idfirme (v214): tbl_web_licence.IdWebLicence, koristi TenantService.GetConnectionString
    kolacici.Upisi(ctx, "ap_idfirme", webLicenca.IdWebLicence.ToString());

    // Učitaj module zastavice iz tenant baze (raw ADO.NET — ne zavisi od EF mapiranja)
    // NULL u bazi = 0 (neaktivan); greška čitanja = 1 (backwards compatible)
    int transportAktivan = 1;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(webLicenca.ConnectionString ?? string.Empty)
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

    kolacici.Upisi(ctx, "ap_transport", transportAktivan.ToString());

    // Učitaj zastavicu e-faktura modula (OpcijaInt13) iz tenant baze
    // NULL u bazi = 0 (neaktivan); greška čitanja = 0 (novo polje, konzervativno)
    int eFakturaAktivna = 0;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(webLicenca.ConnectionString ?? string.Empty)
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

    kolacici.Upisi(ctx, "ap_efaktura", eFakturaAktivna.ToString());

    return new LoginRezultatInterno(true, null, webLicenca.ConnectionString ?? string.Empty, webLicenca.Naziv ?? string.Empty, korisnik.IdKorisnika, korisnik.Privilegija);
}

// API — Prijava (JSON/fetch) — zadržano za kompatibilnost
app.MapPost("/api/auth/login", async (LoginPodaci podaci, MasterDbContext db, HttpContext ctx, KolacicService kolacici) =>
{
    var rez = await PrijaviKorisnikaAsync(podaci.Email, podaci.Password, db, ctx, kolacici);

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
app.MapPost("/api/auth/login-form", async (HttpContext ctx, MasterDbContext db, KolacicService kolacici) =>
{
    var form     = await ctx.Request.ReadFormAsync();
    var email    = form["email"].ToString();
    var password = form["password"].ToString();

    var rez = await PrijaviKorisnikaAsync(email, password, db, ctx, kolacici);

    if (!rez.Success)
        return Results.Redirect($"/login?greska={Uri.EscapeDataString(rez.Poruka ?? "Greška pri prijavljivanju.")}");

    return Results.Redirect(rez.Privilegija == 9 ? "/ds" : "/dashboard");
});

// ============================================================================
// API — Odjava: briše cookie i vraća na /login
// ============================================================================
app.MapGet("/api/auth/logout", (HttpContext ctx, KolacicService kolacici) =>
{
    kolacici.Obrisi(ctx, "ap_conn");
    kolacici.Obrisi(ctx, "ap_firma");
    kolacici.Obrisi(ctx, "ap_ime");
    kolacici.Obrisi(ctx, "ap_priv");
    kolacici.Obrisi(ctx, "ap_user");
    kolacici.Obrisi(ctx, "ap_transport");
    kolacici.Obrisi(ctx, "ap_efaktura");
    kolacici.Obrisi(ctx, "ap_licence");
    kolacici.Obrisi(ctx, "ap_idfirme");
    kolacici.Obrisi(ctx, "ap_impersonate");
    return Results.Redirect("/login");
});

// ============================================================================
// API — Sinhronizacija ap_ime kolačića posle promene imena na /moj-nalog.
// ap_ime se koristi i za "Obračunao"/"Sastavio" na štampama (runtime ime), pa ne
// sme ostati zastareo posle izmene. Ime se čita IZ BAZE (ap_user kolačić je
// potpisan, ali ime se ne uzima iz query stringa) — sesija se ne dodiruje,
// korisnik NIJE odjavljen.
// ============================================================================
app.MapGet("/api/auth/azuriraj-ime", async (HttpContext ctx, MasterDbContext db, KolacicService kolacici) =>
{
    var idKorisnika = int.TryParse(kolacici.Procitaj(ctx, "ap_user"), out var uid) ? uid : 0;
    if (idKorisnika > 0)
    {
        var ime = await db.WebKorisnici.AsNoTracking()
            .Where(k => k.IdKorisnika == idKorisnika)
            .Select(k => k.Ime)
            .FirstOrDefaultAsync();

        if (ime is not null)
            kolacici.Upisi(ctx, "ap_ime", ime);
    }

    return Results.Redirect("/moj-nalog");
});

// ============================================================================
// API — Superadmin: uđi u firmu (impersonacija) / vrati se u panel
// ============================================================================
// Kolačići se ne mogu postaviti iz Blazor interaktivne komponente (odgovor je već
// poslat), zato sve ide preko HTTP endpointa + forceLoad navigacije (isti razlog
// kao kod /api/auth/login-form).
app.MapGet("/api/superadmin/udji", async (int id, HttpContext ctx, MasterDbContext db, KolacicService kolacici) =>
{
    // SIGURNOSNA PROVERA — ne veruj kolačiću ap_priv, proveri stvarno stanje u master bazi.
    var idKorisnika = int.TryParse(kolacici.Procitaj(ctx, "ap_user"), out var uid) ? uid : 0;

    var korisnik = await db.WebKorisnici.AsNoTracking()
        .FirstOrDefaultAsync(k => k.IdKorisnika == idKorisnika);

    if (korisnik is null || korisnik.Aktivan != 1 || korisnik.Privilegija != 9)
        return Results.Redirect("/login");

    // v215 — impersonacija ide preko tbl_web_licence (id = IdWebLicence), ne više tbl_licence.
    var webLicenca = await db.WebLicence.AsNoTracking()
        .FirstOrDefaultAsync(l => l.IdWebLicence == id);

    if (webLicenca is null || string.IsNullOrEmpty(webLicenca.ConnectionString))
        return Results.Redirect("/ds?greska=" + Uri.EscapeDataString("Licenca nema connection string"));

    kolacici.Upisi(ctx, "ap_idfirme",     id.ToString());
    kolacici.Upisi(ctx, "ap_firma",       webLicenca.Naziv ?? string.Empty);
    kolacici.Upisi(ctx, "ap_priv",        "1"); // ap_user/ap_ime se NE diraju — aplikacija se ponaša kao normalna prijava
    kolacici.Upisi(ctx, "ap_impersonate", "1");

    // Isti raw ADO upiti na tenant tbl_Podesavanja kao u PrijaviKorisnikaAsync.
    // Ako upit pukne, oba idu na "0" — ulazak u firmu se ne ruši zbog toga.
    int transportAktivan = 0;
    int eFakturaAktivna  = 0;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(webLicenca.ConnectionString ?? string.Empty)
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

    kolacici.Upisi(ctx, "ap_transport", transportAktivan.ToString());
    kolacici.Upisi(ctx, "ap_efaktura",  eFakturaAktivna.ToString());

    return Results.Redirect("/dashboard");
});

// ============================================================================
// API — Superadmin: "Nastavi na svoju aplikaciju" — NIJE impersonacija tuđe firme,
// superadmin ulazi u SVOJU matičnu firmu preko SVOG članstva (tbl_web_clanstvo).
// Zato ide kroz novi model (tbl_web_licence), ne kroz tbl_licence kao /udji — id ovde
// je IdWebLicence, a ap_impersonate se NE postavlja (GetConnectionString onda ide
// normalnom, članstvom-provereno granom, ne impersonation granom).
// ============================================================================
app.MapGet("/api/superadmin/mojafirma", async (int id, HttpContext ctx, MasterDbContext db, KolacicService kolacici) =>
{
    var idKorisnika = int.TryParse(kolacici.Procitaj(ctx, "ap_user"), out var uid) ? uid : 0;

    var korisnik = await db.WebKorisnici.AsNoTracking()
        .FirstOrDefaultAsync(k => k.IdKorisnika == idKorisnika);

    if (korisnik is null || korisnik.Aktivan != 1 || korisnik.Privilegija != 9)
        return Results.Redirect("/login");

    // Ne veruj query param-u bez provere — id mora biti firma u kojoj superadmin
    // stvarno ima aktivno članstvo.
    var clanstvo = await db.WebClanstva.AsNoTracking()
        .Include(c => c.Licenca)
        .FirstOrDefaultAsync(c => c.IdKorisnika == idKorisnika && c.IdWebLicence == id && c.Aktivan);

    var webLicenca = clanstvo?.Licenca;
    if (webLicenca is null || !webLicenca.Aktivna || string.IsNullOrEmpty(webLicenca.ConnectionString))
        return Results.Redirect("/ds?greska=" + Uri.EscapeDataString("Matična firma nije dostupna."));

    kolacici.Obrisi(ctx, "ap_impersonate"); // NIJE impersonacija
    kolacici.Upisi(ctx, "ap_idfirme", webLicenca.IdWebLicence.ToString());
    kolacici.Upisi(ctx, "ap_firma",   webLicenca.Naziv ?? string.Empty);
    kolacici.Upisi(ctx, "ap_priv",    "1"); // ap_user/ap_ime se NE diraju

    // Isti raw ADO upiti na tenant tbl_Podesavanja kao u PrijaviKorisnikaAsync/udji.
    int transportAktivan = 0;
    int eFakturaAktivna  = 0;
    try
    {
        var tenantCsb = new SqlConnectionStringBuilder(webLicenca.ConnectionString ?? string.Empty)
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

    kolacici.Upisi(ctx, "ap_transport", transportAktivan.ToString());
    kolacici.Upisi(ctx, "ap_efaktura",  eFakturaAktivna.ToString());

    return Results.Redirect("/dashboard");
});

app.MapGet("/api/superadmin/izadji", async (HttpContext ctx, MasterDbContext db, KolacicService kolacici) =>
{
    // SIGURNOSNA PROVERA — ista kao /api/superadmin/udji, ne veruj kolačiću
    // ap_priv. Bez ovoga bilo koji impersonirani korisnik (ap_priv=1) mogao
    // je pozvati ovaj endpoint i sam sebi postaviti ap_priv=9.
    var idKorisnika = int.TryParse(kolacici.Procitaj(ctx, "ap_user"), out var uid) ? uid : 0;

    var korisnik = await db.WebKorisnici.AsNoTracking()
        .FirstOrDefaultAsync(k => k.IdKorisnika == idKorisnika);

    if (korisnik is null || korisnik.Aktivan != 1 || korisnik.Privilegija != 9)
    {
        kolacici.Obrisi(ctx, "ap_impersonate");
        return Results.Redirect("/dashboard");
    }

    kolacici.Obrisi(ctx, "ap_idfirme");
    kolacici.Obrisi(ctx, "ap_firma");
    kolacici.Obrisi(ctx, "ap_impersonate");
    kolacici.Obrisi(ctx, "ap_transport");
    kolacici.Obrisi(ctx, "ap_efaktura");

    kolacici.Upisi(ctx, "ap_priv", "9");

    return Results.Redirect("/ds");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginPodaci(string Email, string Password);

record LoginRezultatInterno(bool Success, string? Poruka, string? ConnectionString, string? NazivFirme, int IdKorisnika, int Privilegija);
