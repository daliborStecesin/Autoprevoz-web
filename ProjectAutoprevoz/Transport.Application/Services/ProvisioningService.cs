using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Kreira novu klijentsku firmu (Faza 1 — samo iz super admin panela):
/// CREATE DATABASE rs{PIB} → izvršava 01_CREATE_kasa_template.sql (embedded
/// resurs iz Transport.Web) → proverava verzijaBaze=213 → upisuje tbl_licence
/// (master) → tbl_imenik (nova baza) → tbl_web_korisnici (master).
/// Bilo koji korak od CREATE DATABASE nadalje koji pukne pokreće čišćenje:
/// brisanje upisane licence/korisnika i DROP DATABASE.
/// </summary>
public class ProvisioningService : IProvisioningService
{
    private const string SkriptaResursIme = "01_CREATE_kasa_template.sql";
    private const int OcekivanaVerzijaBaze = 213;

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

    public async Task<ProvisioningRezultat> KreirajFirmuAsync(
        string pib, string nazivFirme, string ime, string prezime, string email, string lozinka)
    {
        pib        = (pib ?? string.Empty).Trim();
        nazivFirme = (nazivFirme ?? string.Empty).Trim();
        ime        = (ime ?? string.Empty).Trim();
        prezime    = (prezime ?? string.Empty).Trim();
        email      = (email ?? string.Empty).Trim().ToLowerInvariant();
        lozinka  ??= string.Empty;

        var formatGreska = ValidirajFormat(pib, nazivFirme, ime, email, lozinka);
        if (formatGreska is not null)
            return new ProvisioningRezultat(false, formatGreska, null, string.Empty);

        var nazivBaze = "rs" + pib;

        var sablon = _config["SuperAdmin:SablonConnectionString"];
        if (string.IsNullOrWhiteSpace(sablon) || !sablon.Contains("{BAZA}"))
            return new ProvisioningRezultat(false,
                "Šablon connection stringa (SuperAdmin:SablonConnectionString) nije podešen u konfiguraciji.",
                null, nazivBaze);

        var masterConn = IzgradiConnString(sablon, "master");
        var noviConn   = IzgradiConnString(sablon, nazivBaze);

        var duplGreska = await ValidirajDuplikateAsync(pib, email, nazivBaze, masterConn);
        if (duplGreska is not null)
            return new ProvisioningRezultat(false, duplGreska, null, nazivBaze);

        var bazaKreirana = false;
        int? idLicence = null;

        try
        {
            _logger.LogInformation("Provisioning: kreiram bazu {NazivBaze} za PIB {Pib}", nazivBaze, pib);
            await KreirajBazuAsync(masterConn, nazivBaze);
            bazaKreirana = true;

            await PostaviRecoverySimpleAsync(masterConn, nazivBaze);

            _logger.LogInformation("Provisioning: izvršavam 01_CREATE skriptu nad {NazivBaze}", nazivBaze);
            var skripta = await UcitajSkriptuAsync();
            await IzvrsiSkriptuAsync(noviConn, skripta);

            _logger.LogInformation("Provisioning: proveravam verzijaBaze nad {NazivBaze}", nazivBaze);
            await ProveriVerzijuAsync(noviConn);

            _logger.LogInformation("Provisioning: upisujem tbl_Podaci u {NazivBaze}", nazivBaze);
            await UpisiPodaciFirmeAsync(noviConn, nazivFirme, pib);

            _logger.LogInformation("Provisioning: upisujem licencu za PIB {Pib}", pib);
            idLicence = await UpisiLicencuAsync(pib, nazivFirme, noviConn);

            _logger.LogInformation("Provisioning: upisujem prvog zaposlenog u {NazivBaze}", nazivBaze);
            var idZaposlenog = await UpisiZaposlenogAsync(noviConn, ime, prezime, email);

            _logger.LogInformation("Provisioning: upisujem prvog web korisnika za IdLicence {IdLicence}", idLicence);
            await UpisiWebKorisnikaAsync(idLicence.Value, idZaposlenog, ime, prezime, email, lozinka);

            _logger.LogInformation(
                "Provisioning: firma {NazivFirme} (PIB {Pib}) uspešno kreirana — baza {NazivBaze}, IdLicence {IdLicence}",
                nazivFirme, pib, nazivBaze, idLicence);

            return new ProvisioningRezultat(true, "Firma je uspešno kreirana.", idLicence, nazivBaze);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Provisioning: greška pri kreiranju firme za PIB {Pib}, baza {NazivBaze} — pokrećem čišćenje.",
                pib, nazivBaze);

            try
            {
                if (idLicence is not null)
                    await ObrisiLicencuAsync(idLicence.Value);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Provisioning: greška pri brisanju licence {IdLicence} u toku čišćenja.", idLicence);
            }

            try
            {
                if (bazaKreirana)
                    await ObrisiBazuAsync(masterConn, nazivBaze);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Provisioning: greška pri brisanju baze {NazivBaze} u toku čišćenja.", nazivBaze);
            }

            return new ProvisioningRezultat(false, $"Greška pri kreiranju firme: {ex.Message}", null, nazivBaze);
        }
    }

    private static string? ValidirajFormat(string pib, string nazivFirme, string ime, string email, string lozinka)
    {
        if (!Regex.IsMatch(pib, "^[0-9]{9}$"))
            return "PIB mora sadržati tačno 9 cifara.";

        if (string.IsNullOrWhiteSpace(nazivFirme))
            return "Naziv firme je obavezan.";

        if (string.IsNullOrWhiteSpace(ime))
            return "Ime je obavezno.";

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            return "Unesite ispravan email.";

        if (lozinka.Length < 6)
            return "Lozinka mora imati minimum 6 karaktera.";

        return null;
    }

    private async Task<string?> ValidirajDuplikateAsync(string pib, string email, string nazivBaze, string masterConn)
    {
        await using var db = await _masterDbFactory.CreateDbContextAsync();

        var licencaPostoji = await db.Licence.AnyAsync(l =>
            l.PIB == pib && l.ConnectionString != null && l.ConnectionString != "");
        if (licencaPostoji)
            return "Firma sa ovim PIB-om je već registrovana. Prijavite se.";

        var emailPostoji = await db.WebKorisnici.AnyAsync(w => w.Email == email);
        if (emailPostoji)
            return "Email je već u upotrebi.";

        if (await BazaPostojiAsync(masterConn, nazivBaze))
            return "Firma sa ovim PIB-om je već registrovana. Prijavite se.";

        return null;
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

    private async Task<int> UpisiLicencuAsync(string pib, string nazivFirme, string noviConn)
    {
        await using var db = await _masterDbFactory.CreateDbContextAsync();

        var licenca = new Licenca
        {
            Naziv            = nazivFirme,
            PIB              = pib,
            ConnectionString = noviConn,
            WebAktivan       = 1,
            DatumLicence     = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            BrojLicenci      = 1,
            Program          = "AUTOPREVOZ",
            TipLicence       = "PROBA",
            Datum            = DateOnly.FromDateTime(DateTime.Today)
        };

        db.Licence.Add(licenca);
        await db.SaveChangesAsync();

        return licenca.IdLicence;
    }

    // Bez ovog reda stranica /podesavanja/podaci-firme nema šta da čita (kreira
    // prazan in-memory zapis pri prvom otvaranju, ali nova firma bi tada morala
    // ručno da ponovo unese Naziv/PIB koje već znamo iz forme za kreiranje).
    private static async Task UpisiPodaciFirmeAsync(string noviConn, string nazivFirme, string pib)
    {
        var opts = new DbContextOptionsBuilder<TransportDbContext>().UseSqlServer(noviConn).Options;
        await using var db = new TransportDbContext(opts, null);

        db.PodaciFirme.Add(new PodaciFirme
        {
            NazivFirme = nazivFirme,
            PIB        = pib,
            Vlasnik    = "SRBIJA"
        });

        await db.SaveChangesAsync();
    }

    private static async Task<int> UpisiZaposlenogAsync(string noviConn, string ime, string prezime, string email)
    {
        var opts = new DbContextOptionsBuilder<TransportDbContext>().UseSqlServer(noviConn).Options;
        await using var db = new TransportDbContext(opts, null);

        var zaposleni = new Vozac
        {
            Ime     = ime,
            Prezime = prezime,
            Mail    = email,
            aktivan = 1
        };

        db.Vozaci.Add(zaposleni);
        await db.SaveChangesAsync();

        return zaposleni.Broj;
    }

    private async Task UpisiWebKorisnikaAsync(
        int idLicence, int idZaposlenog, string ime, string prezime, string email, string lozinka)
    {
        await using var db = await _masterDbFactory.CreateDbContextAsync();

        var hasher = new PasswordHasher<object>();
        var hash   = hasher.HashPassword(new object(), lozinka);

        var korisnik = new WebKorisnik
        {
            IdLicence      = idLicence,
            IdZaposlenog   = idZaposlenog,
            Ime            = $"{ime} {prezime}".Trim(),
            Email          = email,
            LozinkaHash    = hash,
            Privilegija    = 1,
            Aktivan        = 1,
            DatumKreiranja = DateTime.Now,
            ZadnjaPrijava  = null
        };

        db.WebKorisnici.Add(korisnik);
        await db.SaveChangesAsync();
    }

    private async Task ObrisiLicencuAsync(int idLicence)
    {
        await using var db = await _masterDbFactory.CreateDbContextAsync();

        var korisnici = await db.WebKorisnici.Where(w => w.IdLicence == idLicence).ToListAsync();
        if (korisnici.Count > 0)
            db.WebKorisnici.RemoveRange(korisnici);

        var licenca = await db.Licence.FirstOrDefaultAsync(l => l.IdLicence == idLicence);
        if (licenca is not null)
            db.Licence.Remove(licenca);

        await db.SaveChangesAsync();
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
}
