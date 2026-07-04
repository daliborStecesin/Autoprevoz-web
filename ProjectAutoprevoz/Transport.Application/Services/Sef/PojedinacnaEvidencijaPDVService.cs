using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Pojedinačna evidencija PDV — READ-ONLY lokalna lista (tbl_IndividualVatRecord).
/// Sinhronizacija/kreiranje/otkazivanje preko SEF-a dolazi u sledećoj fazi.
/// </summary>
public class PojedinacnaEvidencijaPDVService : IPojedinacnaEvidencijaPDVService
{
    private readonly TransportDbContext _db;

    public PojedinacnaEvidencijaPDVService(TransportDbContext db)
    {
        _db = db;
    }

    public async Task<List<IndividualVatRecord>> GetListaAsync(
        string? pibFilter,
        string? brojDokumentaFilter,
        string? tipDokumentaFilter,
        string? statusFilter,
        DateTime? datumOd,
        DateTime? datumDo)
    {
        var q = _db.IndividualVatRecords.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(pibFilter))
            q = q.Where(x => x.pibPartnera != null && x.pibPartnera.Contains(pibFilter));

        if (!string.IsNullOrWhiteSpace(brojDokumentaFilter))
            q = q.Where(x => x.documentNumber != null && x.documentNumber.Contains(brojDokumentaFilter));

        if (!string.IsNullOrWhiteSpace(tipDokumentaFilter) && tipDokumentaFilter != "SVI TIPOVI")
            q = q.Where(x => x.documentType == tipDokumentaFilter);

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "SVI STATUSI")
            q = q.Where(x => x.status == statusFilter);

        if (datumOd.HasValue)
        {
            var od = datumOd.Value.Date;
            q = q.Where(x => x.statusChangeDate >= od);
        }

        if (datumDo.HasValue)
        {
            var doKraj = datumDo.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(x => x.statusChangeDate <= doKraj);
        }

        return await q.OrderByDescending(x => x.statusChangeDate).ToListAsync();
    }
}
