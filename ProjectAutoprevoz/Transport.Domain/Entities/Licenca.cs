namespace Transport.Domain.Entities;

public class Licenca
{
    public int IdLicence { get; set; }
    public string? Naziv { get; set; }
    public string? ConnectionString { get; set; }
    public int WebAktivan { get; set; }
    public DateOnly? DatumLicence { get; set; }
}
