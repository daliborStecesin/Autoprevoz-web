using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za dnevnice vozača
/// </summary>
public interface IDnevnicaService
{
    Task<List<Dnevnica>> GetSveDnevniceAsync();
    Task<Dnevnica?> GetDnevnicaByIdAsync(int id);
    Task<Dnevnica> CreateDnevnicaAsync(Dnevnica dnevnica);
    Task<Dnevnica> UpdateDnevnicaAsync(Dnevnica dnevnica);
    Task DeleteDnevnicaAsync(int id);
    Task<int> CalculateBrojDanaAsync(DateTime datumIzlaska, DateTime datumUlaska);
}
