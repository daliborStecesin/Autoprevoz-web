using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface IPojedinacnaEvidencijaPDVService
{
    /// Čitanje iz lokalne baze (tbl_IndividualVatRecord), bez poziva ka SEF-u.
    /// Svi parametri opciono filtriraju; ako su svi null, vraća sve (sortirano statusChangeDate DESC).
    Task<List<IndividualVatRecord>> GetListaAsync(
        string? pibFilter,
        string? brojDokumentaFilter,
        string? tipDokumentaFilter,
        string? statusFilter,
        DateTime? datumOd,
        DateTime? datumDo);
}
