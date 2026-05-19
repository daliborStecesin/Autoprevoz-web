namespace Transport.Domain.Helpers;

/// <summary>
/// Format žiro računa u Srbiji: XXX-XXXXXXXXXXXXX-XX
///   XXX          = šifra banke (3 cifre)
///   XXXXXXXXXXXXX = broj računa (tačno 13 cifara, dopuni 0 s lijeva ako kraće)
///   XX           = kontrolni broj (2 cifre)
/// Primjer: 340-0000011428194-95
/// </summary>
public static class ZiroRacunHelper
{
    /// <summary>
    /// Sklapa žiro račun iz tri dijela u standardni format.
    /// </summary>
    public static string Formatiraj(string? sifraBanke, string? brojRacuna, string? kontrolniBroj)
    {
        if (string.IsNullOrWhiteSpace(brojRacuna)) return string.Empty;

        var srednji = brojRacuna.Trim().PadLeft(13, '0');
        var sifra   = (sifraBanke    ?? string.Empty).Trim();
        var ctrl    = (kontrolniBroj ?? string.Empty).Trim();

        if (!string.IsNullOrEmpty(sifra) && !string.IsNullOrEmpty(ctrl))
            return $"{sifra}-{srednji}-{ctrl}";

        // Ako nemamo sve dijelove — vrati samo padovani srednji broj
        return srednji;
    }

    /// <summary>
    /// Parsira žiro račun iz standardnog formata "XXX-XXXXXXXXXXXXX-XX".
    /// Vraća (sifraBanke, brojRacuna, kontrolniBroj).
    /// </summary>
    public static (string SifraBanke, string BrojRacuna, string KontrolniBroj) Parsiraj(string? racun)
    {
        if (string.IsNullOrWhiteSpace(racun))
            return (string.Empty, string.Empty, string.Empty);

        var d = racun.Trim().Split('-');
        return d.Length == 3
            ? (d[0], d[1], d[2])
            : (string.Empty, racun.Trim(), string.Empty);
    }

    /// <summary>
    /// Normalizuje žiro račun — osigurava standardni format ako je unesen bez crtice.
    /// </summary>
    public static string Normalizuj(string? racun)
    {
        if (string.IsNullOrWhiteSpace(racun)) return string.Empty;
        var (s, b, k) = Parsiraj(racun);
        return Formatiraj(s, b, k);
    }
}
