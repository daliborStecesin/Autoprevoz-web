using Transport.Application.DTOs;

namespace Transport.Application.Interfaces;

public interface INbsKursService
{
    /// <summary>EUR srednji kurs za dati datum (AKTIVA, rateType=2).</summary>
    Task<decimal> GetKursAsync(DateTime datum);

    /// <summary>Sve valute sa kursne liste za dati datum.</summary>
    Task<List<KursValuteDto>> GetSveValuteAsync(DateTime datum);

    /// <summary>Sve valute sa porukom greške za dijagnostiku.</summary>
    Task<(List<KursValuteDto> Lista, string? Greska)> GetSveValuteSaGreskamAsync(DateTime datum);
}
