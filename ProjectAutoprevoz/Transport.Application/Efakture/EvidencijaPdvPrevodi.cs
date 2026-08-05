namespace Transport.Application.Efakture;

/// <summary>
/// Prevodi numeričkih SEF v2 kodova (Pojedinačna/Zbirna evidencija PDV) u srpski
/// tekst — isti duh kao statusPrevod/vatPeriodPrevod na desktopu (Class_eFakturaPrevodi.cs).
/// Nepoznat/null kod vraća null (pozivalac odlučuje šta raditi sa nepoznatom vrednošću).
/// </summary>
public static class EvidencijaPdvPrevodi
{
    public static string? Status(int? kod) => kod switch
    {
        0  => "U pripremi",
        10 => "Evidentirano",
        20 => "Korigovano",
        30 => "Ponisteno",
        _  => null
    };

    public static string? Period(int? kod) => kod switch
    {
        1  => "Januar",
        2  => "Februar",
        3  => "Mart",
        4  => "April",
        5  => "Maj",
        6  => "Jun",
        7  => "Jul",
        8  => "Avgust",
        9  => "Septembar",
        10 => "Oktobar",
        11 => "Novembar",
        12 => "Decembar",
        13 => "Prvi kvartal",
        14 => "Drugi kvartal",
        15 => "Treći kvartal",
        16 => "Četvrti kvartal",
        _  => null
    };
}
