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

    /// Povlači zbirne evidencije PDV sa SEF-a (Public API v2, vat-recording/group) za
    /// dati period i upisuje/ažurira lokalno (tbl_GroupVatRecord), upsert po idGroupVat.
    /// Vraća broj novo upisanih i broj ažuriranih redova. Baca izuzetak na grešku SEF poziva
    /// — pozivalac (UI) hvata i prikazuje poruku.
    Task<(int upisano, int azurirano)> SinhronizujSaSefaAsync(DateTime od, DateTime doDatum);
}
