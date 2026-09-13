using Microsoft.AspNetCore.DataProtection;

namespace Transport.Web.Services;

// JEDINO mesto koje čita/piše/briše aplikacione kolačiće (ap_*). Vrednost se
// potpisuje/enkriptuje preko IDataProtectionProvider — korisnik ne može ručno
// izmeniti kolačić (npr. ap_user) i predstaviti se kao neko drugi, jer izmenjena
// vrednost ne prolazi Unprotect. Imena i logika prijave/odjave se NE menjaju,
// samo način upisa/čitanja sirove vrednosti.
public class KolacicService
{
    private readonly IDataProtector _protector;
    private readonly int _trajanjeDana;

    public KolacicService(IDataProtectionProvider provider, IConfiguration config)
    {
        _protector = provider.CreateProtector("Autoprevoz.Kolacici");
        _trajanjeDana = config.GetValue<int?>("Kolacici:TrajanjeDana") ?? 30;
    }

    public void Upisi(HttpContext ctx, string ime, string vrednost)
    {
        var opts = new CookieOptions
        {
            HttpOnly    = true,
            SameSite    = SameSiteMode.Lax,
            // Test server je http — hardkodovano true bi tiho izbacilo kolačiće (browser ih ne šalje na http).
            Secure      = ctx.Request.IsHttps,
            IsEssential = true,
            Path        = "/",
            Expires     = DateTimeOffset.UtcNow.AddDays(_trajanjeDana)
        };
        ctx.Response.Cookies.Append(ime, _protector.Protect(vrednost), opts);
    }

    // Nikad ne baca — stari nepotpisani kolačić, izmenjena vrednost ili promenjeni
    // ključevi (drugi deploy) sve vraćaju null, što gura korisnika na /login.
    public string? Procitaj(HttpContext ctx, string ime)
    {
        var sirova = ctx.Request.Cookies[ime];
        if (string.IsNullOrEmpty(sirova)) return null;

        try
        {
            return _protector.Unprotect(sirova);
        }
        catch
        {
            return null;
        }
    }

    public void Obrisi(HttpContext ctx, string ime)
    {
        ctx.Response.Cookies.Delete(ime, new CookieOptions { Path = "/" });
    }
}
