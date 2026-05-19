using Transport.Application.DTOs;

namespace Transport.Application.Interfaces;

public interface INbsPartnerService
{
    Task<List<NbsKompanija>> PretraziAsync(long? mb = null, string? pib = null, string? naziv = null);
    Task<(List<NbsKompanija> Rezultati, string? Greska)> PretraziSaDetaljimaAsync(long? mb = null, string? pib = null, string? naziv = null);
}
