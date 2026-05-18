namespace Transport.Domain.Entities;

public class WebKorisnik
{
    public int IdKorisnika { get; set; }
    public int IdLicence { get; set; }
    public string? Ime { get; set; }
    public string? Email { get; set; }
    public string? LozinkaHash { get; set; }
    public int Privilegija { get; set; }
    public int Aktivan { get; set; }
    public Licenca? Licenca { get; set; }
}
