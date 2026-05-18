using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Servis za faktuke i sve vrste računa
/// </summary>
public class RacunService : IRacunService
{
    private readonly TransportDbContext _context;

    public RacunService(TransportDbContext context)
    {
        _context = context;
    }

    public async Task<List<Racun>> GetSveRacuneAsync()
    {
        return await _context.Racuni
            .Include(r => r.Stavke)
            .OrderByDescending(r => r.DatumDokumenta)
            .ToListAsync();
    }

    public async Task<Racun?> GetRacunByIdAsync(int broj)
    {
        return await _context.Racuni
            .Include(r => r.Stavke)
            .FirstOrDefaultAsync(r => r.Broj == broj);
    }

    public async Task<Racun> CreateRacunAsync(Racun racun)
    {
        racun.DatumUnosa = DateTime.Now;
        racun.Brisano = 0;
        
        _context.Racuni.Add(racun);
        await _context.SaveChangesAsync();
        
        return racun;
    }

    public async Task<Racun> UpdateRacunAsync(Racun racun)
    {
        racun.DatumIzmene = DateTime.Now;
        
        _context.Racuni.Update(racun);
        await _context.SaveChangesAsync();
        
        return racun;
    }

    public async Task DeleteRacunAsync(int broj)
    {
        var racun = await GetRacunByIdAsync(broj);
        if (racun != null)
        {
            racun.Brisano = 1; // Soft delete
            await UpdateRacunAsync(racun);
        }
    }

    public async Task<decimal> CalculateRacunTotalsAsync(Racun racun)
    {
        // Preračunaj zbir stavaka
        var stavke = await _context.StavkeRacuna
            .Where(s => s.BrojRacuna == racun.Broj)
            .ToListAsync();

        racun.BrutoIznos = stavke.Sum(s => s.Iznos);
        racun.PDV = stavke.Sum(s => s.IznosPDV);
        racun.Ukupno = stavke.Sum(s => s.IznosUkupno);

        return racun.Ukupno;
    }
}
