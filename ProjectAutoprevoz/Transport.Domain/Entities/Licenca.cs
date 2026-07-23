namespace Transport.Domain.Entities;

public class Licenca
{
    public int IdLicence { get; set; }
    public string? Naziv { get; set; }
    public string? PIB { get; set; }
    public string? ConnectionString { get; set; }
    // Nullable — u tbl_licence postoje (starije/legacy) redovi sa NULL vrednošću.
    public int? WebAktivan { get; set; }
    public DateOnly? DatumLicence { get; set; }
    public string? PorukaKupcu { get; set; }
}
