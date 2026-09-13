namespace Transport.Domain.Helpers;

/// <summary>
/// Jedinstvena lista zemalja i njihovih koda (KodDrzave na tbl_web_licence, koristi se
/// i za prefiks baze rs{PIB}/me{PIB}/...). Izbor zemlje u formama AUTOMATSKI postavlja
/// kod — sprečava neslaganje zemlje i prefiksa baze. Koristi je WebLicencaDialog i (od
/// prompta 7b) forma "Nova firma" — ne duplirati ovu listu.
/// </summary>
public static class ZemljeHelper
{
    public static readonly IReadOnlyList<(string Naziv, string Kod)> Zemlje =
    [
        ("SRBIJA",               "rs"),
        ("CRNA GORA",            "me"),
        ("BOSNA I HERCEGOVINA",  "ba"),
        ("SEVERNA MAKEDONIJA",   "mk"),
        ("SLOVENIJA",            "si"),
        ("HRVATSKA",             "hr"),
    ];

    public static string KodZaZemlju(string? naziv)
    {
        if (string.IsNullOrWhiteSpace(naziv)) return string.Empty;

        foreach (var (n, kod) in Zemlje)
            if (string.Equals(n, naziv, StringComparison.OrdinalIgnoreCase))
                return kod;

        return string.Empty;
    }
}
