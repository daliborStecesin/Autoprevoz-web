using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis koji upisuje/ažurira/briše red kartice partnera (tbl_Kartica) za
/// dati račun. Aplikacija je jedini upisnik kartice (SQL trigger uklonjen).
/// Metode samo STAGE-uju izmene u DbContext-u (Add/Modified) — ne pozivaju
/// SaveChangesAsync, kako bi pozivajuća strana mogla da sačuva račun i
/// karticu u istoj transakciji/SaveChanges pozivu.
/// </summary>
public interface IKarticaService
{
    /// <summary>
    /// Ubaci ili ažuriraj red kartice za dati račun (1 račun = 1 red kartice).
    /// </summary>
    Task UpsertIzRacuna(Racun racun);

    /// <summary>
    /// Fizički briše red(ove) kartice za dati račun (Remove, ne soft delete).
    /// Vraća poruku o grešci ako postoje vezane uplate (u tom slučaju ništa
    /// nije izmenjeno), ili null ako je uspešno.
    /// </summary>
    Task<string?> ObrisiIzRacuna(int brojRacuna);
}
