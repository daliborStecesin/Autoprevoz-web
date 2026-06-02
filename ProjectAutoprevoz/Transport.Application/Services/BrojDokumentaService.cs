namespace Transport.Application.Services;

public static class BrojDokumentaService
{
    /// <summary>
    /// Generiše broj dokumenta po formatu.
    /// Tokeni: broj, godina4, godina2, mesec, dan
    /// Redosled zamene: godina4 pre godina2 da ne bi došlo do delimične zamene.
    /// </summary>
    public static string Formatiraj(
        int broj,
        string? format,
        int minCifara = 0,
        DateTime? datum = null)
    {
        var d = datum ?? DateTime.Today;

        string brojStr = broj.ToString();
        if (minCifara > brojStr.Length)
            brojStr = brojStr.PadLeft(minCifara, '0');

        string godina4 = d.Year.ToString();
        string godina2 = d.ToString("yy");
        string mesec   = d.Month.ToString();
        string dan     = d.Day.ToString();

        if (string.IsNullOrWhiteSpace(format))
            return $"{brojStr}-{godina4}";

        return format
            .Replace("godina4", godina4)
            .Replace("godina2", godina2)
            .Replace("mesec",   mesec)
            .Replace("dan",     dan)
            .Replace("broj",    brojStr);
    }

    public static string Preview(int trenutniBroj, string? format, int minCifara = 0)
        => Formatiraj(trenutniBroj, format, minCifara);
}
