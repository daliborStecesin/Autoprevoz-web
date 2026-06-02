using Microsoft.EntityFrameworkCore;
using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

public class SefService : ISefService
{
    private readonly SefApiClient      _api;
    private readonly TransportDbContext _db;

    public SefService(SefApiClient api, TransportDbContext db)
    {
        _api = api;
        _db  = db;
    }

    private async Task<(string apiKey, string tipServera)> GetSettings()
    {
        var pod = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        return (pod?.sefApiKey ?? string.Empty, pod?.sefTipServera ?? "DEMO");
    }

    // ── Povlači sa SEF i upisuje/ažurira lokalnu bazu ────────────────────────
    public async Task<int> OsveziClanove()
    {
        var (apiKey, tipServera) = await GetSettings();

        var lista = await _api.GetAsync<List<TaxExemptionDto>>(
            apiKey, tipServera,
            "sales-invoice/getValueAddedTaxExemptionReasonList");

        if (lista is null or { Count: 0 }) return 0;

        int obradjeno = 0;
        foreach (var dto in lista)
        {
            var red = await _db.TaxExemptions
                .FirstOrDefaultAsync(x => x.ReasonId == dto.ReasonId);

            if (red is null)
            {
                _db.TaxExemptions.Add(MapDtoToEntity(dto));
            }
            else
            {
                red.KeyClan      = dto.Key;
                red.Law          = dto.Law;
                red.Article      = dto.Article;
                red.Paragraph    = dto.Paragraph;
                red.Point        = dto.Point;
                red.Subpoint     = dto.Subpoint;
                red.Text         = dto.Text;
                red.FreeFormNote = dto.FreeFormNote;
                red.ActiveFrom   = dto.ActiveFrom;
                red.ActiveTo     = dto.ActiveTo;
                red.Category     = dto.Category;
            }
            obradjeno++;
        }

        await _db.SaveChangesAsync();
        return obradjeno;
    }

    // ── Čitanje iz lokalne baze ───────────────────────────────────────────────
    public async Task<List<TaxExemption>> GetSviClanovi(string? filter)
    {
        var q = _db.TaxExemptions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLower();
            q = q.Where(x =>
                (x.Text    != null && x.Text.ToLower().Contains(f)) ||
                (x.KeyClan != null && x.KeyClan.ToLower().Contains(f)));
        }

        return await q.OrderBy(x => x.ReasonId).ToListAsync();
    }

    public async Task<List<TaxExemption>> GetUUpotrebi()
        => await _db.TaxExemptions
            .AsNoTracking()
            .Where(x => x.Koristi == 1)
            .OrderBy(x => x.ReasonId)
            .ToListAsync();

    // ── Upravljanje upotrebom ─────────────────────────────────────────────────
    public async Task DodajUUpotrebu(int reasonId)
    {
        var red = await _db.TaxExemptions.FirstOrDefaultAsync(x => x.ReasonId == reasonId);
        if (red is null) return;
        red.Koristi = 1;
        await _db.SaveChangesAsync();
    }

    public async Task UkloniIzUpotrebe(int reasonId)
    {
        var red = await _db.TaxExemptions.FirstOrDefaultAsync(x => x.ReasonId == reasonId);
        if (red is null) return;
        red.Koristi = 0;
        await _db.SaveChangesAsync();
    }

    public async Task ObrisiVanUpotrebe()
    {
        var redovi = await _db.TaxExemptions
            .Where(x => x.Koristi != 1)
            .ToListAsync();
        _db.TaxExemptions.RemoveRange(redovi);
        await _db.SaveChangesAsync();
    }

    // ── Mapiranje ─────────────────────────────────────────────────────────────
    private static TaxExemption MapDtoToEntity(TaxExemptionDto dto) => new()
    {
        ReasonId     = dto.ReasonId,
        KeyClan      = dto.Key,
        Law          = dto.Law,
        Article      = dto.Article,
        Paragraph    = dto.Paragraph,
        Point        = dto.Point,
        Subpoint     = dto.Subpoint,
        Text         = dto.Text,
        FreeFormNote = dto.FreeFormNote,
        ActiveFrom   = dto.ActiveFrom,
        ActiveTo     = dto.ActiveTo,
        Category     = dto.Category,
        Koristi      = 0
    };
}
