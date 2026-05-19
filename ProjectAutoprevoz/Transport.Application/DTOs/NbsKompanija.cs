namespace Transport.Application.DTOs;

public class NbsKompanija
{
    public string? Naziv        { get; set; }
    public string? Pib          { get; set; }
    public string? MaticniBroj  { get; set; }
    public string? Adresa       { get; set; }
    public string? Mesto        { get; set; }
    public List<NbsRacun> Racuni { get; set; } = [];
}

public class NbsRacun
{
    public string? ZiroRacun { get; set; }
    public string? Banka     { get; set; }
    public bool    Izabran   { get; set; }
}

public record NbsIzbor(NbsKompanija Kompanija, List<NbsRacun> IzabraniRacuni);
