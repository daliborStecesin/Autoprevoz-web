namespace Transport.Application.Interfaces;

/// <summary>
/// Centralni log brisanja (tbl_log_brisanja) — ko/kad/forma/opis za svako
/// brisanje u aplikaciji. Zabelezi() samo STAGE-uje INSERT u DbContext-u
/// (Add) — ne poziva SaveChangesAsync, da bi log i samo brisanje bili
/// upisani u istoj transakciji od strane pozivajuće stranice.
/// Log se nikad ne briše/menja (samo INSERT).
/// </summary>
public interface ILogBrisanjaService
{
    Task Zabelezi(string formaModul, string opis);
}
