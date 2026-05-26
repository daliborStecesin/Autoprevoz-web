using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class DnevnicaService : IDnevnicaService
{
    private readonly TransportDbContext _context;

    public DnevnicaService(TransportDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dnevnica>> GetSveDnevniceAsync()
    {
        return await _context.Dnevnice
            .OrderByDescending(d => d.datumPolaska)
            .ToListAsync();
    }

    public async Task<Dnevnica?> GetDnevnicaByIdAsync(int id)
    {
        return await _context.Dnevnice
            .FirstOrDefaultAsync(d => d.IdDnevnica == id);
    }

    public async Task<Dnevnica> CreateDnevnicaAsync(Dnevnica dnevnica)
    {
        _context.Dnevnice.Add(dnevnica);
        await _context.SaveChangesAsync();
        return dnevnica;
    }

    public async Task<Dnevnica> UpdateDnevnicaAsync(Dnevnica dnevnica)
    {
        _context.Dnevnice.Update(dnevnica);
        await _context.SaveChangesAsync();
        return dnevnica;
    }

    public async Task DeleteDnevnicaAsync(int id)
    {
        var dnevnica = await GetDnevnicaByIdAsync(id);
        if (dnevnica != null)
        {
            _context.Dnevnice.Remove(dnevnica);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> CalculateBrojDanaAsync(DateTime datumIzlaska, DateTime datumUlaska)
    {
        var ts = datumUlaska - datumIzlaska;
        return await Task.FromResult((int)Math.Ceiling(ts.TotalDays) + 1);
    }
}
