using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Infrastructure.Data;

namespace Transport.Web.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _http;
    private readonly MasterDbContext _master;
    private readonly KolacicService _kolacici;

    private string? _cachedConn;
    private string? _cachedFirma;
    private string? _cachedIme;
    private int?    _cachedUserId;
    private int?    _cachedIdFirme;
    private bool?   _cachedImpersonate;
    private int?    _cachedRola;
    private (bool SamoCitanje, DateTime? DatumDo)? _cachedLicencaStatus;

    public TenantService(IHttpContextAccessor http, MasterDbContext master, KolacicService kolacici)
    {
        _http     = http;
        _master   = master;
        _kolacici = kolacici;
    }

    // Kolačići se čitaju SAMO ovde — kroz KolacicService (potpisano/enkriptovano).
    // HttpContext je null posle prerendera u interaktivnom Blazor Server circuit-u,
    // pa se mora pročitati i keširati pri prvom (SSR) čitanju.
    private string? ProcitajKolacic(string ime) =>
        _http.HttpContext is null ? null : _kolacici.Procitaj(_http.HttpContext, ime);

    public void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija)
    {
        _cachedConn   = connectionString;
        _cachedFirma  = nazivFirme;
        _cachedUserId = idKorisnika;
    }

    public string GetConnectionString()
    {
        if (_cachedConn is not null) return _cachedConn;

        var idFirme = GetIdFirme();
        if (idFirme <= 0)
        {
            _cachedConn = string.Empty;
            return _cachedConn;
        }

        // Superadmin impersonacija (v215) — ide preko istog tbl_web_licence kao i sve ostalo,
        // ap_idfirme je UVEK IdWebLicence, nema više dvostrukog značenja. Bez provere članstva
        // (superadmin impersonira TUĐU firmu, nema svoje članstvo u njoj — vidi umesto toga
        // /api/superadmin/mojafirma za superadminovu SOPSTVENU firmu).
        if (JeImpersonacija())
        {
            _cachedConn = _master.WebLicence
                .Where(l => l.IdWebLicence == idFirme)
                .Select(l => l.ConnectionString)
                .FirstOrDefault() ?? string.Empty;
            return _cachedConn;
        }

        // Connection string se ne čuva u cookie-ju (security + veličina cookie-ja na mobilnom) —
        // učitava se iz master baze preko idFirme pri svakom requestu. Čita se SAMO ako korisnik
        // ima aktivno članstvo u toj firmi (i firma je aktivna) — kolačići nisu potpisani, ne
        // sme se verovati ap_idfirme bez provere da korisnik stvarno pripada toj firmi.
        var idKorisnika = GetIdKorisnika();
        var conn = (from c in _master.WebClanstva
                    join l in _master.WebLicence on c.IdWebLicence equals l.IdWebLicence
                    where c.IdKorisnika == idKorisnika
                       && c.IdWebLicence == idFirme
                       && c.Aktivan && l.Aktivna
                    select l.ConnectionString).FirstOrDefault();

        if (conn is null)
        {
            Logout();
            return _cachedConn ?? string.Empty;
        }

        _cachedConn = conn;
        return _cachedConn;
    }

    public string GetNazivFirme()
    {
        if (_cachedFirma is not null) return _cachedFirma;
        _cachedFirma = ProcitajKolacic("ap_firma") ?? string.Empty;
        return _cachedFirma;
    }

    public int GetPrivilegija()
    {
        var val = ProcitajKolacic("ap_priv") ?? "0";
        return int.TryParse(val, out var p) ? p : 0;
    }

    public int GetIdKorisnika()
    {
        if (_cachedUserId.HasValue) return _cachedUserId.Value;
        var val = ProcitajKolacic("ap_user") ?? "0";
        _cachedUserId = int.TryParse(val, out var id) ? id : 0;
        return _cachedUserId.Value;
    }

    // Rezolucija tenant-a preko tbl_web_licence (v214/v215) — IdWebLicence firme u koju je
    // korisnik prijavljen (preko tbl_web_clanstvo) ili koju superadmin impersonira. UVEK
    // IdWebLicence, u svim granama — nema više dvostrukog značenja (tbl_licence.IdLicence).
    public int GetIdFirme()
    {
        if (_cachedIdFirme.HasValue) return _cachedIdFirme.Value;
        var val = ProcitajKolacic("ap_idfirme") ?? "0";
        _cachedIdFirme = int.TryParse(val, out var id) ? id : 0;
        return _cachedIdFirme.Value;
    }

    public string GetImeKorisnika()
    {
        if (_cachedIme is not null) return _cachedIme;
        _cachedIme = ProcitajKolacic("ap_ime") ?? string.Empty;
        return _cachedIme;
    }

    public bool GetTransportModulAktivan()
    {
        var val = ProcitajKolacic("ap_transport") ?? "1";
        return val != "0"; // default: aktivan (1); 0 = isključen
    }

    public bool GetEFakturaAktivna()
    {
        var val = ProcitajKolacic("ap_efaktura") ?? "0";
        return val == "1"; // default: neaktivan (0); 1 = aktivan
    }

    public bool JeImpersonacija()
    {
        // Keširano po istom obrascu kao ap_firma/ap_user/ap_idfirme/ap_ime — HttpContext
        // je NULL posle prerendera u interaktivnom Blazor Server circuit-u, pa se mora
        // pročitati i upamtiti pri prvom (SSR) čitanju, inače uvek vraća false.
        if (_cachedImpersonate.HasValue) return _cachedImpersonate.Value;
        var val = ProcitajKolacic("ap_impersonate") ?? "0";
        _cachedImpersonate = val == "1";
        return _cachedImpersonate.Value;
    }

    public bool IsAuthenticated() => !string.IsNullOrEmpty(GetConnectionString());

    // IdWebRole korisnika u trenutnoj firmi (1=Vlasnik, 2=Administrator, 3=Operater; 0=nema
    // aktivno članstvo). Privilegija se čita IZ MASTER BAZE (ne iz kolačića ap_priv) — ap_priv
    // se spušta na "1" i za impersonaciju i za superadminov ulazak u svoju firmu, pa kolačić
    // ne otkriva stvarnu privilegiju. Superadmin (Privilegija>=9) se tretira kao Vlasnik (1).
    public async Task<int> RolaTrenutneFirme()
    {
        if (_cachedRola.HasValue) return _cachedRola.Value;

        var idKorisnika = GetIdKorisnika();
        if (idKorisnika <= 0)
        {
            _cachedRola = 0;
            return 0;
        }

        var privilegija = await _master.WebKorisnici.AsNoTracking()
            .Where(k => k.IdKorisnika == idKorisnika)
            .Select(k => k.Privilegija)
            .FirstOrDefaultAsync();

        if (privilegija >= 9)
        {
            _cachedRola = 1;
            return 1;
        }

        var idFirme = GetIdFirme();
        var rola = await _master.WebClanstva.AsNoTracking()
            .Where(c => c.IdKorisnika == idKorisnika
                     && c.IdWebLicence == idFirme
                     && c.Aktivan)
            .Select(c => (int?)c.IdWebRole)
            .FirstOrDefaultAsync();

        _cachedRola = rola ?? 0;
        return _cachedRola.Value;
    }

    // Upravljanje web korisnicima (registracija/pristup/rola) — sme SAMO vlasnik firme
    // (IdWebRole=1 u trenutnoj firmi) ili superadmin (RolaTrenutneFirme svodi oboje na 1).
    public async Task<bool> JeVlasnikTrenutneFirme() => await RolaTrenutneFirme() == 1;

    // SamoCitanje/DatumDo trenutne firme — jedan upit, keširan, koriste ga JeSamoCitanje()
    // i DatumDoTrenutneFirme(). Isti obrazac kao RolaTrenutneFirme().
    private async Task<(bool SamoCitanje, DateTime? DatumDo)> UcitajLicencaStatus()
    {
        if (_cachedLicencaStatus.HasValue) return _cachedLicencaStatus.Value;

        var idFirme = GetIdFirme();
        if (idFirme <= 0)
        {
            _cachedLicencaStatus = (false, null);
            return _cachedLicencaStatus.Value;
        }

        var red = await _master.WebLicence.AsNoTracking()
            .Where(l => l.IdWebLicence == idFirme)
            .Select(l => new { l.SamoCitanje, l.DatumDo })
            .FirstOrDefaultAsync();

        _cachedLicencaStatus = red is null ? (false, null) : (red.SamoCitanje, red.DatumDo);
        return _cachedLicencaStatus.Value;
    }

    // true ako je SamoCitanje=1 ILI je DatumDo prošao — ista logika kao
    // vw_web_pristup.EfektivnoSamoCitanje. VAŽI I ZA SUPERADMINA (uključujući
    // impersonaciju) — bez izuzetka, ap_idfirme je uvek IdWebLicence pa je upit isti za sve.
    public async Task<bool> JeSamoCitanje()
    {
        var (samoCitanje, datumDo) = await UcitajLicencaStatus();
        return samoCitanje || (datumDo.HasValue && datumDo.Value.Date < DateTime.Today);
    }

    // DatumDo trenutne firme — za najavu isteka (traka u MainLayout-u).
    public async Task<DateTime?> DatumDoTrenutneFirme()
    {
        var (_, datumDo) = await UcitajLicencaStatus();
        return datumDo;
    }

    public void Logout()
    {
        _cachedConn      = string.Empty;
        _cachedFirma     = string.Empty;
        _cachedIme       = string.Empty;
        _cachedUserId    = 0;
        _cachedIdFirme   = 0;
        _cachedImpersonate = false;
        _cachedRola      = null;
        _cachedLicencaStatus = null;
    }
}
