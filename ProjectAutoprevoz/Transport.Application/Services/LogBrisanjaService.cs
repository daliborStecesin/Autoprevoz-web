using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class LogBrisanjaService : ILogBrisanjaService
{
    private readonly TransportDbContext _context;
    private readonly ITenantService _tenantService;

    public LogBrisanjaService(TransportDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public Task Zabelezi(string formaModul, string opis)
    {
        _context.LogoviBrisanja.Add(new LogBrisanja
        {
            datumVreme   = DateTime.Now,
            idKorisnika  = _tenantService.GetIdKorisnika(),
            imeKorisnika = _tenantService.GetImeKorisnika(),
            formaModul   = formaModul,
            opis         = opis
        });

        return Task.CompletedTask;
    }
}
