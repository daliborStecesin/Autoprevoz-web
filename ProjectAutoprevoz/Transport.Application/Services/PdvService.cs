using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using System.Net.Http;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

/// <summary>
/// Servis za PDV, eFakturu i poreske obaveze
/// </summary>
public class PdvService : IPdvService
{
    private readonly TransportDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public PdvService(TransportDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<VatDeductionRecord>> GetSveEvidencije()
    {
        return await _context.VatDeductionRecords
            .OrderByDescending(v => v.DatumRacuna)
            .ToListAsync();
    }

    public async Task<VatDeductionRecord> CreateEvidencijaAsync(VatDeductionRecord evidencija)
    {
        evidencija.DatumUnosa = DateTime.Now;
        
        _context.VatDeductionRecords.Add(evidencija);
        await _context.SaveChangesAsync();
        
        return evidencija;
    }

    public async Task<List<ObavestenjePP>> GetSvaObavestenjaAsync()
    {
        return await _context.ObavestenjaPP
            .OrderByDescending(o => o.Datum)
            .ToListAsync();
    }

    public async Task<ObavestenjePP> CreateObavestenjeAsync(ObavestenjePP obavestenje)
    {
        obavestenje.DatumUnosa = DateTime.Now;
        
        _context.ObavestenjaPP.Add(obavestenje);
        await _context.SaveChangesAsync();
        
        return obavestenje;
    }

    /// <summary>
    /// Preuzmi EUR kurs sa NBS za dati datum
    /// </summary>
    public async Task<decimal> GetEurKursAsync(DateTime datum)
    {
        try
        {
            // Privremeno vraćamo fiksnu vrednost - u fazi 2 integracija sa NBS API
            return 117.50m; // Privremeni placeholder
        }
        catch
        {
            return 117.50m; // Default vrednost ako greška
        }
    }
}
