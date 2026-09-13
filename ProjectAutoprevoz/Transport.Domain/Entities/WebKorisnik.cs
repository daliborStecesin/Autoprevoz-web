namespace Transport.Domain.Entities;

// IdZaposlenog NIJE ovde — zaposleni je pojam PO FIRMI (klijentska baza je odvojena
// po firmi), a WebKorisnik je globalan (isti nalog kroz sve firme kojima pripada).
// Izvor istine je WebClanstvo.IdZaposlenog (po članstvu). Kolona tbl_web_korisnici.
// IdZaposlenog ostaje u bazi, briše se u v215 zajedno sa IdLicence.
public class WebKorisnik
{
    public int IdKorisnika { get; set; }
    public string? Ime { get; set; }
    public string? Email { get; set; }
    public string? LozinkaHash { get; set; }
    public int Privilegija { get; set; }
    public int Aktivan { get; set; }
    public DateTime? DatumKreiranja { get; set; }
    public DateTime? ZadnjaPrijava { get; set; }
}
