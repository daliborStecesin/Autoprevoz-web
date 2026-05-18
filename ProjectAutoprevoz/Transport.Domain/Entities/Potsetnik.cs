namespace Transport.Domain.Entities;

public class Potsetnik
{
    public int     IdPotsetnik    { get; set; }
    public string? Vrsta          { get; set; }
    public string? Opis           { get; set; }
    public DateOnly? DatumIzrade  { get; set; }
    public DateOnly? DatumPotsenika { get; set; }
    public int?    Ponavljanje    { get; set; }
}
