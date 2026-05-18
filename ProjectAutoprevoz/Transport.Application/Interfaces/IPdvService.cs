using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za PDV, eFakturu i poreske obaveze
/// </summary>
public interface IPdvService
{
    Task<List<VatDeductionRecord>> GetSveEvidencije();
    Task<VatDeductionRecord> CreateEvidencijaAsync(VatDeductionRecord evidencija);
    Task<List<ObavestenjePP>> GetSvaObavestenjaAsync();
    Task<ObavestenjePP> CreateObavestenjeAsync(ObavestenjePP obavestenje);
    Task<decimal> GetEurKursAsync(DateTime datum);
}
