using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za transportne naloge
/// </summary>
public interface ITransportService
{
    Task<List<NalogPrevoz>> GetSveNalogeAsync();
    Task<NalogPrevoz?> GetNalogByIdAsync(int id);
    Task<NalogPrevoz> CreateNalogAsync(NalogPrevoz nalog);
    Task<NalogPrevoz> UpdateNalogAsync(NalogPrevoz nalog);
    Task DeleteNalogAsync(int id);
    
    // Putni nalozi
    Task<List<PutniNalogKamion>> GetPutneNalogeAsync();
    Task<PutniNalogKamion?> GetPutniNalogByIdAsync(int id);
    Task<PutniNalogKamion> CreatePutniNalogAsync(PutniNalogKamion nalog);
    Task<decimal> CalculateGorivoPotrosnjuAsync(decimal km, decimal potrosnjaPer100km);
}
