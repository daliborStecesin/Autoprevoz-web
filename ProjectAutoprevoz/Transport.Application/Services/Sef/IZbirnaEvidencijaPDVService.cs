using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface IZbirnaEvidencijaPDVService
{
    /// Čitanje iz lokalne baze (tbl_GroupVatRecord), bez poziva ka SEF-u.
    /// Svi parametri opciono filtriraju; ako su svi null, vraća sve (sortirano statusChangeDate DESC).
    Task<List<GroupVatRecord>> GetListaAsync(
        string? brojObracunaFilter,
        string? statusFilter,
        DateTime? datumOd,
        DateTime? datumDo);
}
