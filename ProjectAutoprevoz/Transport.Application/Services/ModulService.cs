using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class ModulService : IModulService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ITenantService _tenant;

    private int?              _cachedZaIdFirme;
    private HashSet<string>?  _cachedModuli;

    public ModulService(IDbContextFactory<MasterDbContext> masterFactory, ITenantService tenant)
    {
        _masterFactory = masterFactory;
        _tenant        = tenant;
    }

    public async Task<HashSet<string>> DozvoljeniModuli()
    {
        var idFirme = _tenant.GetIdFirme();

        // Keširano po circuit-u, ali vezano za idFirme (ne samo "prvi poziv") — kad
        // TenantService.Logout() resetuje ap_idfirme na 0 (isto kao RolaTrenutneFirme),
        // keš ovde automatski postaje nevažeći na sledećem pozivu, bez cirkularne
        // zavisnosti IModulService → ITenantService → IModulService.
        if (_cachedModuli is not null && _cachedZaIdFirme == idFirme)
            return _cachedModuli;

        // Kratkotrajan, izolovan kontekst preko IDbContextFactory (ne deljeni scoped
        // MasterDbContext) — NavMenu poziva ovo na SVAKOJ stranici, paralelno sa
        // inicijalizacijom same stranice (koja često i sama koristi MasterDbContext,
        // direktno ili preko TenantService), pa bi deljena instanca izazvala
        // "A second operation was started on this context instance...".
        WebLicenca? licenca = null;
        if (idFirme > 0)
        {
            await using var db = await _masterFactory.CreateDbContextAsync();
            licenca = await db.WebLicence.AsNoTracking().FirstOrDefaultAsync(l => l.IdWebLicence == idFirme);
        }

        HashSet<string> moduli;
        if (licenca is not null)
        {
            moduli = new HashSet<string>();
            if (licenca.ModulTure)        moduli.Add("TURE");
            if (licenca.ModulRadniNalozi) moduli.Add("RADNI_NALOZI");
            if (licenca.ModulLager)       moduli.Add("LAGER");
            if (licenca.ModulEFaktura)    moduli.Add("EFAKTURA");
            if (licenca.ModulEOtpremnica) moduli.Add("EOTPREMNICA");
        }
        else
        {
            // Fail-closed — nema tbl_web_licence reda za ovu firmu (uključujući impersonaciju,
            // v215: impersonacija ide isključivo preko tbl_web_licence, nema više legacy grane
            // za tbl_licence). Licenca je licenca, i za superadmina.
            moduli = new HashSet<string>();
        }

        _cachedZaIdFirme = idFirme;
        _cachedModuli    = moduli;
        return moduli;
    }

    public async Task<bool> JeDozvoljen(string kodModula)
        => (await DozvoljeniModuli()).Contains(kodModula);
}
