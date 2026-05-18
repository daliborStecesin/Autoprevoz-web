using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za faktuke i sve vrste računa
/// </summary>
public interface IRacunService
{
    Task<List<Racun>> GetSveRacuneAsync();
    Task<Racun?> GetRacunByIdAsync(int broj);
    Task<Racun> CreateRacunAsync(Racun racun);
    Task<Racun> UpdateRacunAsync(Racun racun);
    Task DeleteRacunAsync(int broj);
    Task<decimal> CalculateRacunTotalsAsync(Racun racun);
}
