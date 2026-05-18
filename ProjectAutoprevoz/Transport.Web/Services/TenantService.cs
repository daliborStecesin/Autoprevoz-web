using Transport.Application.Interfaces;

namespace Transport.Web.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _http;

    private string? _cachedConn;
    private string? _cachedFirma;

    public TenantService(IHttpContextAccessor http) => _http = http;

    public void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija)
    {
        _cachedConn  = connectionString;
        _cachedFirma = nazivFirme;
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

    public bool IsAuthenticated() => !string.IsNullOrEmpty(GetConnectionString());

    public void Logout()
    {
        _cachedConn  = string.Empty;
        _cachedFirma = string.Empty;
    }
}
