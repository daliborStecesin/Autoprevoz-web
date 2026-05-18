using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Servis za upravljanje partnerima
/// </summary>
public class PartnerService : IPartnerService
{
    private readonly TransportDbContext _context;

    public PartnerService(TransportDbContext context)
    {
        _context = context;
    }

    public async Task<List<Partner>> GetSviPartneriAsync()
    {
        return await _context.Partneri
            .OrderBy(p => p.Naziv)
            .ToListAsync();
    }

    public async Task<Partner?> GetPartnerByIdAsync(int broj)
    {
        return await _context.Partneri
            .FirstOrDefaultAsync(p => p.Broj == broj);
    }

    public async Task<Partner> CreatePartnerAsync(Partner partner)
    {
        partner.DatumUnosa = DateTime.Now;
        partner.Brisano = 0;
        
        _context.Partneri.Add(partner);
        await _context.SaveChangesAsync();
        
        return partner;
    }

    public async Task<Partner> UpdatePartnerAsync(Partner partner)
    {
        partner.DatumIzmene = DateTime.Now;
        
        _context.Partneri.Update(partner);
        await _context.SaveChangesAsync();
        
        return partner;
    }

    public async Task DeletePartnerAsync(int broj)
    {
        var partner = await GetPartnerByIdAsync(broj);
        if (partner != null)
        {
            partner.Brisano = 1; // Soft delete
            await UpdatePartnerAsync(partner);
        }
    }

    public async Task<bool> CheckPibUniqueAsync(string pib, int? excludeId = null)
    {
        var exists = await _context.Partneri
            .Where(p => p.PIB == pib && (excludeId == null || p.Broj != excludeId))
            .AnyAsync();
        
        return !exists;
    }
}
