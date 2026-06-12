namespace Transport.Application.Interfaces;

public interface IKursService
{
    /// <summary>
    /// Kurs EUR za platu na dati datum (datum putnog naloga).
    /// SRBIJA → NBS srednji kurs za taj datum (fallback na tbl_Podesavanja.kursEur).
    /// Ostale države → tbl_Podesavanja.kursEur (ručni kurs; za EUR-zemlje = 1).
    /// </summary>
    Task<decimal> GetKursAsync(DateTime datum);
}
