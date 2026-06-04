using Transport.Application.Interfaces;

namespace Transport.Web.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _http;

    private string? _cachedConn;
    private string? _cachedFirma;
    private string? _cachedIme;
    private int?    _cachedUserId;
    private int?    _cachedLicenceId;

    public TenantService(IHttpContextAccessor http) => _http = http;

    public void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija, int idLicence = 0)
    {
        _cachedConn      = connectionString;
        _cachedFirma     = nazivFirme;
        _cachedUserId    = idKorisnika;
        _cachedLicenceId = idLicence;
    }

    public string GetConnectionString()
    {
        if (_cachedConn is not null) return _cachedConn;
        _cachedConn = _http.HttpContext?.Request.Cookies["ap_conn"] ?? string.Empty;
        return _cachedConn;
    }

    public string GetNazivFirme()
    {
        if (_cachedFirma is not null) return _cachedFirma;
        _cachedFirma = _http.HttpContext?.Request.Cookies["ap_firma"] ?? string.Empty;
        return _cachedFirma;
    }

    public int GetPrivilegija()
    {
        var val = _http.HttpContext?.Request.Cookies["ap_priv"] ?? "0";
        return int.TryParse(val, out var p) ? p : 0;
    }

    public int GetIdKorisnika()
    {
        if (_cachedUserId.HasValue) return _cachedUserId.Value;
        var val = _http.HttpContext?.Request.Cookies["ap_user"] ?? "0";
        _cachedUserId = int.TryParse(val, out var id) ? id : 0;
        return _cachedUserId.Value;
    }

    public int GetIdLicence()
    {
        if (_cachedLicenceId.HasValue) return _cachedLicenceId.Value;
        var val = _http.HttpContext?.Request.Cookies["ap_licence"] ?? "0";
        _cachedLicenceId = int.TryParse(val, out var id) ? id : 0;
        return _cachedLicenceId.Value;
    }

    public string GetImeKorisnika()
    {
        if (_cachedIme is not null) return _cachedIme;
        _cachedIme = _http.HttpContext?.Request.Cookies["ap_ime"] ?? string.Empty;
        return _cachedIme;
    }

    public bool GetTransportModulAktivan()
    {
        var val = _http.HttpContext?.Request.Cookies["ap_transport"] ?? "1";
        return val != "0"; // default: aktivan (1); 0 = isključen
    }

    public bool IsAuthenticated() => !string.IsNullOrEmpty(GetConnectionString());

    public void Logout()
    {
        _cachedConn      = string.Empty;
        _cachedFirma     = string.Empty;
        _cachedIme       = string.Empty;
        _cachedUserId    = 0;
        _cachedLicenceId = 0;
    }
}
