namespace Transport.Domain.Entities;

public class WebClanstvo
{
    public int IdClanstva { get; set; }
    public int IdKorisnika { get; set; }
    public int IdWebLicence { get; set; }
    public int IdWebRole { get; set; }
    public bool JeVlasnik { get; set; }

    // Pokazuje u KLIJENTSKU bazu (tbl_Zaposleni) — bez FK/navigacije, master ne poznaje klijentsku šemu.
    public int? IdZaposlenog { get; set; }
    public bool Aktivan { get; set; }
    public DateTime DatumDodavanja { get; set; }
    public int? DodaoKorisnik { get; set; }
    public string? Napomena { get; set; }

    public WebLicenca? Licenca { get; set; }
    public WebRola? Rola { get; set; }
}
