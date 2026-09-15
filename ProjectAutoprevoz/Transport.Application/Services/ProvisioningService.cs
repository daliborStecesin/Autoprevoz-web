using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Domain.Helpers;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Kreira novu WEB klijentsku firmu (Faza 1 — samo iz super admin panela):
/// CREATE DATABASE {kodDrzave}{PIB} → izvršava 01_CREATE_kasa_template.sql (embedded
/// resurs iz Transport.Web) → proverava verzijaBaze=215 → upisuje tbl_Podaci/
/// tbl_zaposleni (klijentska baza) → tbl_web_licence/tbl_web_korisnici/
/// tbl_web_clanstvo (master). Vidi KreirajWebFirmuAsync za rollback po koraku.
/// </summary>
public class ProvisioningService : IProvisioningService
{
    private const string SkriptaResursIme = "01_CREATE_kasa_template.sql";
    private const int OcekivanaVerzijaBaze = 215;

    private readonly IDbContextFactory<MasterDbContext> _masterDbFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ProvisioningService> _logger;
    private readonly Assembly _resursAssembly;

    public ProvisioningService(
        IDbContextFactory<MasterDbContext> masterDbFactory,
        IConfiguration config,
        ILogger<ProvisioningService> logger,
        Assembly resursAssembly)
    {
        _masterDbFactory = masterDbFactory;
        _config          = config;
        _logger          = logger;
        _resursAssembly  = resursAssembly;
    }

    private static async Task<bool> BazaPostojiAsync(string masterConn, string nazivBaze)
    {
        await using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM sys.databases WHERE name = @naziv";
        cmd.Parameters.AddWithValue("@naziv", nazivBaze);

        var rezultat = await cmd.ExecuteScalarAsync();
        return rezultat is not null;
    }

    private static string IzgradiConnString(string sablon, string baza)
    {
        var csb = new SqlConnectionStringBuilder(sablon.Replace("{BAZA}", baza))
        {
            TrustServerCertificate = true,
            Encrypt = false
        };
        return csb.ConnectionString;
    }

    // Šablon iz appsettings.json "SqlServeri" — BEZ {BAZA} placeholder-a, bez Initial
    // Catalog. Ime baze se dodaje ovde, preko SqlConnectionStringBuilder (radi i za
    // "Server=" i za "Data Source=" — sinonimi za istu stavku u connection stringu).
    private static string IzgradiConnStringOdSablona(string sablonBezBaze, string imeBaze)
    {
        var csb = new SqlConnectionStringBuilder(sablonBezBaze)
        {
            InitialCatalog         = imeBaze,
            TrustServerCertificate = true,
            Encrypt                = false
        };
        return csb.ConnectionString;
    }

    // Server za NOVU bazu — iz appsettings.json "SqlServeri" (izabran po ključu iz forme,
    // default prvi u sekciji) ako sekcija postoji; inače stari fallback (SuperAdmin:
    // SablonConnectionString, {BAZA} placeholder) — "kao do sad", da se ne razbije postojeće
    // ponašanje ako SqlServeri nije podešeno.
    private (string? Greska, string MasterConn, string NoviConn) OdrediServerZaNovuBazu(string? izabranServer, string imeBaze)
    {
        var serveri = _config.GetSection("SqlServeri").GetChildren().ToList();

        if (serveri.Count > 0)
        {
            var kljuc  = string.IsNullOrWhiteSpace(izabranServer) ? serveri[0].Key : izabranServer;
            var stavka = serveri.FirstOrDefault(s => s.Key == kljuc);
            if (stavka is null || string.IsNullOrWhiteSpace(stavka.Value))
                return ($"nepoznat SQL server '{kljuc}' (sekcija SqlServeri).", string.Empty, string.Empty);

            try
            {
                var sablonBezBaze = stavka.Value!;
                return (null,
                    IzgradiConnStringOdSablona(sablonBezBaze, "master"),
                    IzgradiConnStringOdSablona(sablonBezBaze, imeBaze));
            }
            catch (Exception ex)
            {
                return ($"server '{kljuc}' ima neispravan connection string u konfiguraciji ({ex.Message}).", string.Empty, string.Empty);
            }
        }

        var sablonStari = _config["SuperAdmin:SablonConnectionString"];
        if (string.IsNullOrWhiteSpace(sablonStari) || !sablonStari.Contains("{BAZA}"))
            return ("ni SqlServeri ni SuperAdmin:SablonConnectionString nisu podešeni u konfiguraciji.", string.Empty, string.Empty);

        return (null, IzgradiConnString(sablonStari, "master"), IzgradiConnString(sablonStari, imeBaze));
    }

    private static async Task KreirajBazuAsync(string masterConn, string nazivBaze)
    {
        await using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandTimeout = 120;
        cmd.CommandText = $"CREATE DATABASE [{nazivBaze}]";
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task PostaviRecoverySimpleAsync(string masterConn, string nazivBaze)
    {
        await using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandTimeout = 30;
        cmd.CommandText = $"ALTER DATABASE [{nazivBaze}] SET RECOVERY SIMPLE";
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<string> UcitajSkriptuAsync()
    {
        await using var stream = _resursAssembly.GetManifestResourceStream(SkriptaResursIme)
            ?? throw new InvalidOperationException(
                $"Embedded resurs '{SkriptaResursIme}' nije pronađen u sklopu {_resursAssembly.GetName().Name}.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    // Seče skriptu po linijama koje su SAMO "GO" (case-insensitive, trim) — isti
    // pristup kao sqlcmd/SSMS batch separator. Prazni batch-evi se preskaču.
    private static IReadOnlyList<string> RazdvojNaBatch(string skripta)
    {
        var linije    = skripta.Replace("\r\n", "\n").Split('\n');
        var batchevi  = new List<string>();
        var trenutni  = new StringBuilder();

        foreach (var linija in linije)
        {
            if (linija.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = trenutni.ToString();
                if (!string.IsNullOrWhiteSpace(batch))
                    batchevi.Add(batch);
                trenutni.Clear();
            }
            else
            {
                trenutni.AppendLine(linija);
            }
        }

        var poslednji = trenutni.ToString();
        if (!string.IsNullOrWhiteSpace(poslednji))
            batchevi.Add(poslednji);

        return batchevi;
    }

    private static async Task IzvrsiSkriptuAsync(string noviConn, string skripta)
    {
        var batchevi = RazdvojNaBatch(skripta);

        await using var conn = new SqlConnection(noviConn);
        await conn.OpenAsync();

        foreach (var batch in batchevi)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText    = batch;
            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task ProveriVerzijuAsync(string noviConn)
    {
        await using var conn = new SqlConnection(noviConn);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText    = "SELECT TOP 1 verzijaBaze FROM tbl_Podesavanja";
        cmd.CommandTimeout = 30;

        var val     = await cmd.ExecuteScalarAsync();
        var verzija = (val is DBNull or null) ? (int?)null : Convert.ToInt32(val);

        if (verzija != OcekivanaVerzijaBaze)
            throw new InvalidOperationException(
                $"Baza je kreirana, ali verzijaBaze = {(verzija?.ToString() ?? "NULL")} (očekivano {OcekivanaVerzijaBaze}) — skripta nije potpuno izvršena.");
    }

    private static async Task ObrisiBazuAsync(string masterConn, string nazivBaze)
    {
        await using var conn = new SqlConnection(masterConn);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandTimeout = 30;
            cmd.CommandText    = $"ALTER DATABASE [{nazivBaze}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandTimeout = 30;
            cmd.CommandText    = $"DROP DATABASE [{nazivBaze}]";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // ========================================================================
    // KreirajWebFirmuAsync (v215/7b) — nova WEB firma: tbl_web_licence +
    // tbl_web_clanstvo (master), tbl_Podaci/tbl_zaposleni (klijentska baza).
    // Logička transakcija: svaki korak koji pukne pokreće čišćenje samo onoga
    // što je OVAJ poziv upisao. Ako bazaVecPostoji, baza se NIKAD ne dira
    // (ni upis, ni brisanje) — postojeći desktop klijent može imati stvarne
    // podatke u tbl_Podaci/tbl_zaposleni, njih ne diramo.
    // ========================================================================
    public async Task<NovaWebFirmaRezultat> KreirajWebFirmuAsync(NovaWebFirmaZahtev z)
    {
        var zemlja        = (z.Zemlja ?? "").Trim();
        var kodDrzave     = (z.KodDrzave ?? "").Trim().ToLowerInvariant();
        var pib           = (z.Pib ?? "").Trim();
        var naziv         = (z.Naziv ?? "").Trim();
        var tipPrograma   = (z.TipPrograma ?? "").Trim();
        var tipLicence    = (z.TipLicence ?? "").Trim();
        var imeVlasnika   = (z.ImeVlasnika ?? "").Trim();
        var emailVlasnika = (z.EmailVlasnika ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(zemlja) || string.IsNullOrWhiteSpace(kodDrzave) ||
            string.IsNullOrWhiteSpace(pib) || string.IsNullOrWhiteSpace(naziv) ||
            string.IsNullOrWhiteSpace(tipPrograma) || string.IsNullOrWhiteSpace(tipLicence) ||
            string.IsNullOrWhiteSpace(imeVlasnika) || string.IsNullOrWhiteSpace(emailVlasnika))
        {
            return Neuspeh("Nedostaju obavezni podaci (zemlja, kod države, PIB, naziv, tip programa/licence, ime/email vlasnika).");
        }

        if (z.BazaVecPostoji && string.IsNullOrWhiteSpace(z.ConnectionString))
            return Neuspeh("Connection string je obavezan kad baza već postoji.");

        // Korak 1 — duplikat po (KodDrzave, Pib).
        await using (var provera = await _masterDbFactory.CreateDbContextAsync())
        {
            var postoji = await provera.WebLicence.AnyAsync(l => l.KodDrzave == kodDrzave && l.Pib == pib);
            if (postoji)
                return Neuspeh($"Web licenca za državu '{kodDrzave}' i PIB {pib} već postoji.");
        }

        string  noviConn;
        string? imeBaze;
        var     bazaKreirana     = false;
        string? masterConnZaDrop = null;

        // Korak 2 — baza.
        if (!z.BazaVecPostoji)
        {
            imeBaze = kodDrzave + pib;

            var (greskaServera, masterConn, noviConnLokalno) = OdrediServerZaNovuBazu(z.SqlServer, imeBaze);
            if (greskaServera is not null)
                return Neuspeh($"Korak 2 (baza): {greskaServera}");

            noviConn         = noviConnLokalno;
            masterConnZaDrop = masterConn;

            try
            {
                if (await BazaPostojiAsync(masterConn, imeBaze))
                    return Neuspeh($"Korak 2 (baza): baza {imeBaze} već postoji na serveru.");

                _logger.LogInformation("KreirajWebFirmuAsync: kreiram bazu {ImeBaze} za PIB {Pib}", imeBaze, pib);
                await KreirajBazuAsync(masterConn, imeBaze);
                bazaKreirana = true;

                await PostaviRecoverySimpleAsync(masterConn, imeBaze);

                var skripta = await UcitajSkriptuAsync();
                await IzvrsiSkriptuAsync(noviConn, skripta);

                await ProveriVerzijuAsync(noviConn);
            }
            catch (Exception ex)
            {
                if (bazaKreirana)
                    await BezbednoObrisiBazuAsync(masterConn, imeBaze);
                return Neuspeh($"Korak 2 (baza): {ex.Message}");
            }
        }
        else
        {
            var csb = new SqlConnectionStringBuilder(z.ConnectionString!)
            {
                TrustServerCertificate = true,
                Encrypt = false
            };
            noviConn = csb.ConnectionString;
            imeBaze  = csb.InitialCatalog;

            try
            {
                var verzija = await ProcitajVerzijuAsync(noviConn);
                if (verzija < OcekivanaVerzijaBaze)
                    return Neuspeh($"Korak 2 (postojeća baza): verzijaBaze = {verzija} (potrebno {OcekivanaVerzijaBaze}) — pusti 02_MIGRACIJA_postojeci_klijent.sql pre nastavka.");

                // Provera identiteta — connection string se kuca ručno, a vezivanje web
                // licence za POGREŠNU bazu znači da klijent vidi tuđe podatke. Jedina brana.
                var (pronadjenNaziv, pronadjenPib) = await ProcitajIdentitetFirmeAsync(noviConn);

                if (!string.IsNullOrWhiteSpace(pronadjenPib))
                {
                    if (!string.Equals(pronadjenPib.Trim(), pib, StringComparison.OrdinalIgnoreCase))
                        return Neuspeh(
                            $"Baza na datom connection string-u pripada firmi {pronadjenNaziv ?? "(nepoznato)"} (PIB {pronadjenPib}), " +
                            $"a registruje se {naziv} (PIB {pib}). Provjerite connection string.");
                }
                else if (!z.PotvrdjenoNepoklapanje)
                {
                    return PotrebnaPotvrdaRezultat(pronadjenNaziv);
                }
            }
            catch (Exception ex)
            {
                return Neuspeh($"Korak 2 (provera konekcije): {ex.Message}");
            }
        }

        // Korak 3 — klijentska baza: tbl_Podaci (samo za novu bazu) + tbl_zaposleni
        // (prvi zaposleni = vlasnik; za postojeću bazu prvo traži postojećeg po emailu).
        int idZaposlenog;
        try
        {
            idZaposlenog = await UpisiPodatkeIZaposlenogAsync(
                noviConn, z, naziv, pib, emailVlasnika, imeVlasnika, zemlja, bazaNova: !z.BazaVecPostoji);
        }
        catch (Exception ex)
        {
            if (bazaKreirana && masterConnZaDrop is not null)
                await BezbednoObrisiBazuAsync(masterConnZaDrop, imeBaze);
            return Neuspeh($"Korak 3 (klijentska baza — tbl_Podaci/tbl_zaposleni): {ex.Message}");
        }

        // Korak 4 — master baza, atomično (jedna transakcija — rollback ako nešto pukne,
        // bez ijedne manuelne DELETE komande; ako je korisnik već postojao, ništa se
        // ne upisuje/menja za njega pa ga rollback transakcije ne dotiče).
        try
        {
            await using var masterDb = await _masterDbFactory.CreateDbContextAsync();
            await using var tx = await masterDb.Database.BeginTransactionAsync();

            var webLicenca = new WebLicenca
            {
                Naziv            = naziv,
                Pib              = pib,
                MaticniBroj      = z.MaticniBroj,
                Adresa           = z.Adresa,
                Mesto            = z.Mesto,
                PostanskiBroj    = z.PostanskiBroj,
                Zemlja           = zemlja,
                KodDrzave        = kodDrzave,
                ImeBaze          = imeBaze,
                ConnectionString = noviConn,
                TipPrograma      = tipPrograma,
                ModulTure        = z.ModulTure,
                ModulRadniNalozi = z.ModulRadniNalozi,
                ModulLager       = z.ModulLager,
                ModulEFaktura    = z.ModulEFaktura,
                ModulEOtpremnica = z.ModulEOtpremnica,
                TipLicence       = tipLicence,
                DatumOd          = z.DatumOd,
                DatumDo          = z.DatumDo,
                MaxKorisnika     = z.MaxKorisnika,
                Aktivna          = true,
                SamoCitanje      = false,
                Telefon          = z.Telefon,
                Email            = emailVlasnika,
                IzvorPrijave     = "SUPERADMIN",
                DatumKreiranja   = DateTime.Now
            };
            masterDb.WebLicence.Add(webLicenca);
            await masterDb.SaveChangesAsync();

            var korisnik            = await masterDb.WebKorisnici.FirstOrDefaultAsync(k => k.Email == emailVlasnika);
            var korisnikNovokreiran = korisnik is null;
            string? generisanaLozinka = null;

            if (korisnik is null)
            {
                generisanaLozinka = LozinkaHelper.Generisi(naziv);
                var hasher = new PasswordHasher<object>();
                korisnik = new WebKorisnik
                {
                    Ime            = imeVlasnika,
                    Email          = emailVlasnika,
                    LozinkaHash    = hasher.HashPassword(new object(), generisanaLozinka),
                    Privilegija    = 1,
                    Aktivan        = 1,
                    DatumKreiranja = DateTime.Now,
                    ZadnjaPrijava  = null
                };
                masterDb.WebKorisnici.Add(korisnik);
                await masterDb.SaveChangesAsync();
            }

            masterDb.WebClanstva.Add(new WebClanstvo
            {
                IdKorisnika    = korisnik.IdKorisnika,
                IdWebLicence   = webLicenca.IdWebLicence,
                IdWebRole      = 1, // Vlasnik
                JeVlasnik      = true,
                IdZaposlenog   = idZaposlenog,
                Aktivan        = true,
                DatumDodavanja = DateTime.Now
            });
            await masterDb.SaveChangesAsync();

            await tx.CommitAsync();

            _logger.LogInformation(
                "KreirajWebFirmuAsync: firma {Naziv} (PIB {Pib}) uspešno kreirana — IdWebLicence {IdWebLicence}, baza {ImeBaze}",
                naziv, pib, webLicenca.IdWebLicence, imeBaze);

            return new NovaWebFirmaRezultat(
                true, "Web firma je uspešno kreirana.",
                webLicenca.IdWebLicence, imeBaze, emailVlasnika, generisanaLozinka, !korisnikNovokreiran);
        }
        catch (Exception ex)
        {
            // Transakcija se automatski poništava (using bez commit) — master ostaje čist.
            if (bazaKreirana && masterConnZaDrop is not null)
                await BezbednoObrisiBazuAsync(masterConnZaDrop, imeBaze);
            return Neuspeh($"Korak 4 (master baza): {ex.Message}");
        }
    }

    private NovaWebFirmaRezultat Neuspeh(string poruka)
    {
        _logger.LogWarning("KreirajWebFirmuAsync: {Poruka}", poruka);
        return new NovaWebFirmaRezultat(false, poruka, null, null, null, null, false);
    }

    private NovaWebFirmaRezultat PotrebnaPotvrdaRezultat(string? pronadjenNaziv)
    {
        var poruka =
            $"tbl_Podaci nema upisan PIB, ne može se automatski potvrditi da je ovo tačna firma " +
            $"(pronađen naziv: '{pronadjenNaziv ?? "(prazno)"}').";
        _logger.LogWarning("KreirajWebFirmuAsync: {Poruka}", poruka);
        return new NovaWebFirmaRezultat(false, poruka, null, null, null, null, false,
            PotrebnaPotvrda: true, PronadjenNaziv: pronadjenNaziv);
    }

    private async Task BezbednoObrisiBazuAsync(string masterConn, string nazivBaze)
    {
        try
        {
            await ObrisiBazuAsync(masterConn, nazivBaze);
        }
        catch (Exception cleanupEx)
        {
            _logger.LogError(cleanupEx, "KreirajWebFirmuAsync: greška pri brisanju baze {NazivBaze} u toku čišćenja.", nazivBaze);
        }
    }

    // Naziv+PIB već upisani u tbl_Podaci postojeće baze — provera identiteta pre
    // nego što se ta baza veže za web licencu (vidi poziv u Koraku 2).
    private static async Task<(string? Naziv, string? Pib)> ProcitajIdentitetFirmeAsync(string noviConn)
    {
        var opts = new DbContextOptionsBuilder<TransportDbContext>()
            .UseSqlServer(noviConn, sql => sql.UseCompatibilityLevel(120)).Options;
        await using var db = new TransportDbContext(opts, null);

        var podaci = await db.PodaciFirme.AsNoTracking().FirstOrDefaultAsync();
        return (podaci?.NazivFirme, podaci?.PIB);
    }

    private static async Task<int> ProcitajVerzijuAsync(string noviConn)
    {
        await using var conn = new SqlConnection(noviConn);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText    = "SELECT TOP 1 verzijaBaze FROM tbl_Podesavanja";
        cmd.CommandTimeout = 10;

        var val = await cmd.ExecuteScalarAsync();
        return (val is DBNull or null) ? 0 : Convert.ToInt32(val);
    }

    // tbl_Podaci se upisuje SAMO za novu bazu (bazaNova) — postojeća desktop baza već
    // ima svoj red, ne dupliramo/ne diramo ga. tbl_zaposleni: ako postoji zaposleni sa
    // istim mail-om (bez obzira na aktivan status), koristi njega; inače upiši novog
    // (vlasnika) i vrati njegov Broj.
    private static async Task<int> UpisiPodatkeIZaposlenogAsync(
        string noviConn, NovaWebFirmaZahtev z, string naziv, string pib,
        string emailVlasnika, string imeVlasnika, string zemlja, bool bazaNova)
    {
        var opts = new DbContextOptionsBuilder<TransportDbContext>()
            .UseSqlServer(noviConn, sql => sql.UseCompatibilityLevel(120)).Options;
        await using var db = new TransportDbContext(opts, null);

        if (bazaNova)
        {
            db.PodaciFirme.Add(new PodaciFirme
            {
                NazivFirme  = naziv,
                PIB         = pib,
                MaticniBroj = z.MaticniBroj,
                Adresa      = z.Adresa,
                Mesto       = z.Mesto,
                PosBroj     = z.PostanskiBroj,
                // "Vlasnik" nosi zemlju firme (IKursService: Vlasnik != "SRBIJA" -> ručni kurs;
                // isti naziv kao PodaciFirme.razor "Država" select). Obavezno — bez ovoga kurs
                // puca za firme van Srbije.
                Vlasnik     = zemlja
            });
            await db.SaveChangesAsync();
        }

        var postojeci = await db.Vozaci.IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Mail != null && v.Mail.ToLower() == emailVlasnika);
        if (postojeci is not null)
            return postojeci.Broj;

        var zaposleni = new Vozac { Ime = imeVlasnika, Mail = emailVlasnika, aktivan = 1 };
        db.Vozaci.Add(zaposleni);
        await db.SaveChangesAsync();
        return zaposleni.Broj;
    }

    // Čitljiva lozinka oblika "PrvaRecNaziva-4cifre" (npr. Prevoz-4821). Hešuje se odmah
    // (PasswordHasher<object>, isti hasher koji login koristi za verifikaciju) — plain
    // tekst se NIGDE ne čuva, vraća se samo kao rezultat ovog poziva. Generator je u
    // Transport.Domain.Helpers.LozinkaHelper — isti koristi i WebKorisnikDialog (reset lozinke).
}
