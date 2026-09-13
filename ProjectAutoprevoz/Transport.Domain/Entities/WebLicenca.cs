namespace Transport.Domain.Entities;

public class WebLicenca
{
    public int IdWebLicence { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Pib { get; set; } = string.Empty;
    public string? MaticniBroj { get; set; }
    public string? Adresa { get; set; }
    public string? Mesto { get; set; }
    public string? PostanskiBroj { get; set; }
    public string Zemlja { get; set; } = string.Empty;
    public string KodDrzave { get; set; } = string.Empty;
    public string? ImeBaze { get; set; }
    public string? ConnectionString { get; set; }
    public string TipPrograma { get; set; } = string.Empty;
    public bool ModulTure { get; set; }
    public bool ModulRadniNalozi { get; set; }
    public bool ModulLager { get; set; }
    public string TipLicence { get; set; } = string.Empty;
    public DateTime? DatumOd { get; set; }
    public DateTime? DatumDo { get; set; }
    public int? MaxKorisnika { get; set; }
    public bool Aktivna { get; set; }
    public bool SamoCitanje { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? IzvorPrijave { get; set; }
    public string? Napomena { get; set; }
    public DateTime DatumKreiranja { get; set; }
}
