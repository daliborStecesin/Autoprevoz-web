using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Transport.Application.Efakture;
using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Zbirna evidencija PDV — lokalna lista (tbl_GroupVatRecord) + SEF sinhronizacija
/// (Public API v2, vat-recording/group). Kreiranje/otkazivanje preko SEF-a dolazi
/// u sledećoj fazi.
/// </summary>
public class ZbirnaEvidencijaPDVService : IZbirnaEvidencijaPDVService
{
    private readonly TransportDbContext _db;
    private readonly SefApiClient       _api;

    public ZbirnaEvidencijaPDVService(TransportDbContext db, SefApiClient api)
    {
        _db  = db;
        _api = api;
    }

    private async Task<(string apiKey, string tipServera)> GetSettings()
    {
        var pod = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        return (pod?.sefApiKey ?? string.Empty, pod?.sefTipServera ?? "DEMO");
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

    // ── SEF sinhronizacija (Public API v2) — upsert po idGroupVat ───────────────
    public async Task<(int upisano, int azurirano)> SinhronizujSaSefaAsync(DateTime od, DateTime doDatum)
    {
        var (apiKey, tipServera) = await GetSettings();

        var endpoint = $"vat-recording/group?dateFrom={od:yyyy-MM-dd}&dateTo={doDatum:yyyy-MM-dd}";
        var json     = await _api.GetStringAsyncV2(apiKey, tipServera, endpoint);

        var dtoLista = JsonSerializer.Deserialize<List<GroupVatSefDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        var upisano   = 0;
        var azurirano = 0;

        foreach (var dto in dtoLista)
        {
            var postojeci = await _db.GroupVatRecords.FirstOrDefaultAsync(x => x.idGroupVat == dto.groupVatId);

            if (postojeci is null)
            {
                _db.GroupVatRecords.Add(new GroupVatRecord
                {
                    idGroupVat             = dto.groupVatId,
                    year                   = dto.year,
                    calculationNumber      = dto.calculationNumber,
                    documentNumber         = null, // grupni odgovor ga nema
                    relatedPartyIdentifier = null, // grupni odgovor ga nema
                    vatPeriodStr           = EvidencijaPdvPrevodi.Period(dto.vatPeriod),
                    recordingDate          = dto.recordingDate,
                    statusChangeDate       = dto.statusChangeDate,
                    vatRecordingStatus     = EvidencijaPdvPrevodi.Status(dto.vatRecordingStatus),
                    createdUtc             = DateTime.UtcNow
                });
                upisano++;
            }
            else
            {
                postojeci.vatRecordingStatus = EvidencijaPdvPrevodi.Status(dto.vatRecordingStatus);
                postojeci.statusChangeDate   = dto.statusChangeDate;
                postojeci.vatPeriodStr       = EvidencijaPdvPrevodi.Period(dto.vatPeriod);
                postojeci.calculationNumber  = dto.calculationNumber;
                postojeci.year               = dto.year;
                azurirano++;
            }
        }

        await _db.SaveChangesAsync();

        return (upisano, azurirano);
    }
}
