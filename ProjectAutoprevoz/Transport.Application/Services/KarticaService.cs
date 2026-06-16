using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Servis koji upisuje/ažurira/briše red kartice partnera (tbl_Kartica) za
/// dati račun. Aplikacija je jedini upisnik kartice (SQL trigger uklonjen).
/// </summary>
public class KarticaService : IKarticaService
{
    private static readonly string[] _platniStatusi = ["UPLATA", "ISPLATA", "INO_UPLATA", "INO_ISPLATA"];

    private readonly TransportDbContext _context;

    public KarticaService(TransportDbContext context)
    {
        _context = context;
    }

    public async Task UpsertIzRacuna(Racun racun)
    {
        var idRacuna = racun.Broj.ToString();
        var status   = MapirajStatus(racun.TipProdaje);

        var postojeci = await _context.Kartice
            .FirstOrDefaultAsync(k => k.IdRacuna == idRacuna);

        if (postojeci is null)
        {
            _context.Kartice.Add(new KarticaPartnera
            {
                IdRacuna      = idRacuna,
                IdPartnera    = racun.IdPartnera?.ToString(),
                BrojRacuna    = racun.BrojRacuna,
                NazivPartnera = racun.Naziv,
                PibPartnera   = racun.PIB,
                DatumRacuna   = racun.DatumRacuna,
                DatumValute   = racun.DatumValute,
                DatumPrometa  = racun.DatumPrometaDo,
                Dug           = racun.SumaRacuna,
                Uplata        = 0,
                Status        = status,
                selektor      = "Kupac",
                Duguje        = racun.SumaRacuna,
                Potrazuje     = 0,
                Saldo         = racun.SumaRacuna,
                Izmiren       = "NE",
                brisano       = 0
            });
        }
        else
        {
            postojeci.IdPartnera    = racun.IdPartnera?.ToString();
            postojeci.BrojRacuna    = racun.BrojRacuna;
            postojeci.NazivPartnera = racun.Naziv;
            postojeci.PibPartnera   = racun.PIB;
            postojeci.DatumRacuna   = racun.DatumRacuna;
            postojeci.DatumValute   = racun.DatumValute;
            postojeci.DatumPrometa  = racun.DatumPrometaDo;
            postojeci.Status        = status;
            postojeci.selektor      = "Kupac";

            // Dug/Duguje/Saldo se ažuriraju ako se iznos računa promenio.
            // Uplata i Izmiren se NE diraju — izmena računa ne sme da
            // obriše već vezane uplate (Preostalo je computed = Dug - Uplata).
            postojeci.Dug     = racun.SumaRacuna;
            postojeci.Duguje  = racun.SumaRacuna;
            postojeci.Saldo   = racun.SumaRacuna;
        }
    }

    public async Task<string?> ObrisiIzRacuna(int brojRacuna)
    {
        var idRacuna = brojRacuna.ToString();

        var redovi = await _context.Kartice
            .Where(k => k.IdRacuna == idRacuna)
            .ToListAsync();

        if (redovi.Any(k => _platniStatusi.Contains(k.Status)))
        {
            var brojRacunaPrikaz = redovi
                .Select(r => r.BrojRacuna)
                .FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) ?? idRacuna;

            return $"Račun {brojRacunaPrikaz} ima vezane uplate. Prvo obriši uplate, pa zatim račun.";
        }

        foreach (var red in redovi)
            red.brisano = 1;

        return null;
    }

    private static string? MapirajStatus(string? tipProdaje) => tipProdaje switch
    {
        "IZLAZ" or "IZLAZ_BP" => "IZLAZ",
        "INOSTRANI" => "INOSTRANI",
        _ => tipProdaje
    };
}
