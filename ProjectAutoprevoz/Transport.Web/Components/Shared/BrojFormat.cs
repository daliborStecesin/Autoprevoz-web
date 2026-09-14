namespace Transport.Web.Components.Shared;

// Jedinstveni format brojeva za cenu/količinu/iznos — v214 je proširio cenu i
// količinu na decimal(18,4) u bazi i EF modelu (tbl_artikli_racuna, tbl_lineItem),
// pa prikaz i unos moraju da prate istu preciznost. Iznosi (osnovica/PDV/ukupno)
// OSTAJU na 2 decimale — to SEF traži, ne dirati.
//
// Format stringovi: '0' = obavezna cifra, '#' = opciona cifra.
//   CenaFormat     — minimum 2, do 4 decimale (npr. 12,50 ili 12,3456)
//   KolicinaFormat — bez minimuma, do 4 decimale (npr. 1 ili 1,1235)
//   IznosFormat    — uvek tačno 2 decimale
public static class BrojFormat
{
    public const string CenaFormat     = "#,##0.00##";
    public const string KolicinaFormat = "#,##0.####";
    public const string IznosFormat    = "#,##0.00";

    public static string FormatCena(this decimal v)      => v.ToString(CenaFormat);
    public static string FormatCena(this decimal? v)     => (v ?? 0m).ToString(CenaFormat);

    public static string FormatKolicinu(this decimal v)  => v.ToString(KolicinaFormat);
    public static string FormatKolicinu(this decimal? v) => (v ?? 0m).ToString(KolicinaFormat);

    public static string FormatIznos(this decimal v)     => v.ToString(IznosFormat);
    public static string FormatIznos(this decimal? v)    => (v ?? 0m).ToString(IznosFormat);
}
