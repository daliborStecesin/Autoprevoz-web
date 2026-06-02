using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface ISefService
{
    /// Povlači listu sa SEF API-ja i upisuje/ažurira lokalu bazu.
    /// Vraća broj obrađenih članova.
    Task<int> OsveziClanove();

    /// Svi članovi iz lokalne baze, opciono filtrirani po tekstu ili ključu.
    Task<List<TaxExemption>> GetSviClanovi(string? filter);

    /// Samo članovi označeni kao Koristi = 1.
    Task<List<TaxExemption>> GetUUpotrebi();

    Task DodajUUpotrebu(int reasonId);
    Task UkloniIzUpotrebe(int reasonId);

    /// Briše sve redove gde Koristi != 1 (čisti bazu od nekorišćenih).
    Task ObrisiVanUpotrebe();
}
