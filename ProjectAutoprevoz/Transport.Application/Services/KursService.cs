using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class KursService : IKursService
{
    private readonly TransportDbContext _context;
    private readonly INbsKursService _nbsKurs;

    public KursService(TransportDbContext context, INbsKursService nbsKurs)
    {
        _context = context;
        _nbsKurs = nbsKurs;
    }

    public async Task<decimal> GetKursAsync(DateTime datum)
    {
        var podaci = await _context.PodaciFirme.AsNoTracking().FirstOrDefaultAsync();
        var pod    = await _context.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        var rucniKurs = pod?.kursEur ?? 0m;

        if (podaci?.Vlasnik == "SRBIJA")
        {
            try
            {
                var kurs = await _nbsKurs.GetKursAsync(datum);
                if (kurs > 0) return kurs;
            }
            catch { }

            return rucniKurs;
        }

        return rucniKurs;
    }
}
