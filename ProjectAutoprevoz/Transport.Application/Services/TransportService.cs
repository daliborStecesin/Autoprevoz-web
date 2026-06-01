using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class TransportService : ITransportService
{
    private readonly TransportDbContext _context;

    public TransportService(TransportDbContext context)
    {
        _context = context;
    }

    public async Task<List<NalogPrevoz>> GetSveNalogeAsync()
    {
        return await _context.NaloziZaPrevoz
            .Include(n => n.Tura)
            .OrderByDescending(n => n.DatumDokumenta)
            .ToListAsync();
    }

    public async Task<NalogPrevoz?> GetNalogByIdAsync(int id)
    {
        return await _context.NaloziZaPrevoz
            .Include(n => n.Tura)
            .FirstOrDefaultAsync(n => n.IdNaloga == id);
    }

    public async Task<NalogPrevoz> CreateNalogAsync(NalogPrevoz nalog)
    {
        nalog.Brisano = 0;
        _context.NaloziZaPrevoz.Add(nalog);
        await _context.SaveChangesAsync();
        return nalog;
    }

    public async Task<NalogPrevoz> UpdateNalogAsync(NalogPrevoz nalog)
    {
        _context.NaloziZaPrevoz.Update(nalog);
        await _context.SaveChangesAsync();
        return nalog;
    }

    public async Task DeleteNalogAsync(int id)
    {
        var nalog = await GetNalogByIdAsync(id);
        if (nalog is not null)
        {
            nalog.Brisano = 1;
            await UpdateNalogAsync(nalog);
        }
    }

    public async Task<List<PutniNalogKamion>> GetPutneNalogeAsync()
    {
        return await _context.PutniNalozi
            .OrderByDescending(p => p.DatumIzlaska)
            .ToListAsync();
    }

    public async Task<PutniNalogKamion?> GetPutniNalogByIdAsync(int id)
    {
        return await _context.PutniNalozi
            .FirstOrDefaultAsync(p => p.IdNaloga == id);
    }

    public async Task<PutniNalogKamion> CreatePutniNalogAsync(PutniNalogKamion nalog)
    {
        _context.PutniNalozi.Add(nalog);
        await _context.SaveChangesAsync();
        return nalog;
    }

    public Task<decimal> CalculateGorivoPotrosnjuAsync(decimal km, decimal potrosnjaPer100km)
    {
        return Task.FromResult(Math.Round(km / 100 * potrosnjaPer100km, 2));
    }
}
