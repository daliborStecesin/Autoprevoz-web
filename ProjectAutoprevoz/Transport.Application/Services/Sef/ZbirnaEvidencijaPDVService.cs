using Microsoft.EntityFrameworkCore;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Zbirna evidencija PDV — READ-ONLY lokalna lista (tbl_GroupVatRecord).
/// Sinhronizacija/kreiranje/otkazivanje preko SEF-a dolazi u sledećoj fazi.
/// </summary>
public class ZbirnaEvidencijaPDVService : IZbirnaEvidencijaPDVService
{
    private readonly TransportDbContext _db;

    public ZbirnaEvidencijaPDVService(TransportDbContext db)
    {
        _db = db;
    }

    public async Task<List<GroupVatRecord>> GetListaAsync(
        string? brojObracunaFilter,
        string? statusFilter,
        DateTime? datumOd,
        DateTime? datumDo)
    {
        var q = _db.GroupVatRecords.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(brojObracunaFilter))
            q = q.Where(x => x.calculationNumber != null && x.calculationNumber.Contains(brojObracunaFilter));

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "SVI STATUSI")
            q = q.Where(x => x.vatRecordingStatus == statusFilter);

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
