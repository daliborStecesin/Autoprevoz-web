using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Servis za transportne naloge
/// </summary>
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
            .Include(n => n.PutniNalozi)
            .OrderByDescending(n => n.DatumDokumenta)
            .ToListAsync();
    }

    public async Task<NalogPrevoz?> GetNalogByIdAsync(int id)
    {
        return await _context.NaloziZaPrevoz
            .Include(n => n.PutniNalozi)
            .FirstOrDefaultAsync(n => n.IdNaloga == id);
    }

    public async Task<NalogPrevoz> CreateNalogAsync(NalogPrevoz nalog)
    {
        nalog.DatumUnosa = DateTime.Now;
        nalog.Brisano = 0;
        
        _context.NaloziZaPrevoz.Add(nalog);
        await _context.SaveChangesAsync();
        
        return nalog;
    }

    public async Task<NalogPrevoz> UpdateNalogAsync(NalogPrevoz nalog)
    {
        nalog.DatumIzmene = DateTime.Now;
        
        _context.NaloziZaPrevoz.Update(nalog);
        await _context.SaveChangesAsync();
        
        return nalog;
    }

    public async Task DeleteNalogAsync(int id)
    {
        var nalog = await GetNalogByIdAsync(id);
        if (nalog != null)
        {
            nalog.Brisano = 1; // Soft delete
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
        nalog.DatumUnosa = DateTime.Now;
        
        _context.PutniNalozi.Add(nalog);
        await _context.SaveChangesAsync();
        
        return nalog;
    }

    public async Task<decimal> CalculateGorivoPotrosnjuAsync(decimal km, decimal potrosnjaPer100km)
    {
        // Preračun: km / 100 * potrosnjaPer100km
        var potrosnja = (km / 100) * potrosnjaPer100km;
        return Math.Round(potrosnja, 2);
    }
}
