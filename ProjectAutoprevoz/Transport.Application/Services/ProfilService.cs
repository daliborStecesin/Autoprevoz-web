using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class ProfilService : IProfilService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ITenantService _tenant;

    private int?    _cachedZaIdFirme;
    private string? _cachedTipPrograma;

    public ProfilService(IDbContextFactory<MasterDbContext> masterFactory, ITenantService tenant)
    {
        _masterFactory = masterFactory;
        _tenant        = tenant;
    }

    public async Task<string> TipPrograma()
    {
        var idFirme = _tenant.GetIdFirme();

        // Keširano po circuit-u, vezano za idFirme — isti obrazac kao ModulService.
        if (_cachedTipPrograma is not null && _cachedZaIdFirme == idFirme)
            return _cachedTipPrograma;

        // Kratkotrajan, izolovan kontekst preko IDbContextFactory (ne deljeni scoped
        // MasterDbContext) — isti razlog kao ModulService (stranica se inicijalizuje
        // paralelno sa ovim pozivom).
        var tipPrograma = "";
        if (idFirme > 0)
        {
            await using var db = await _masterFactory.CreateDbContextAsync();
            tipPrograma = await db.WebLicence.AsNoTracking()
                .Where(l => l.IdWebLicence == idFirme)
                .Select(l => l.TipPrograma)
                .FirstOrDefaultAsync() ?? "";
        }

        _cachedZaIdFirme   = idFirme;
        _cachedTipPrograma = tipPrograma;
        return tipPrograma;
    }

    public async Task<bool> JeTransport() => await TipPrograma() == "TRANSPORT";

    public async Task<bool> PrikaziDatumUtovara() => await JeTransport();
}
