namespace Transport.Domain.Entities;

public interface IAuditable
{
    int?      Uneo        { get; set; }
    DateTime? DatumUnosa  { get; set; }
    int?      Izmenio     { get; set; }
    DateTime? DatumIzmene { get; set; }
}
