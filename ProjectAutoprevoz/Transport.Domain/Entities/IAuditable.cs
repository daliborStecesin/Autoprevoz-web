namespace Transport.Domain.Entities;

/// Audit interfejs. Uneo (int) je izostavljen jer stare WinForms tabele
/// imaju varchar kolonu "uneo" koja bi izazvala InvalidCastException.
/// DatumUnosa prati kad je zapis kreiran, Izmenio+DatumIzmene ko/kad je
/// poslednji put izmenjen.
public interface IAuditable
{
    DateTime? DatumUnosa  { get; set; }
    int?      Izmenio     { get; set; }
    DateTime? DatumIzmene { get; set; }
}
