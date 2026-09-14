namespace Transport.Application.Interfaces;

public interface IModulService
{
    /// <summary>
    /// Da li je modul (kod: "TURE", "RADNI_NALOZI", "LAGER", "EFAKTURA", "EOTPREMNICA")
    /// dozvoljen za trenutnu firmu (ap_idfirme). Čita tbl_web_licence — nikad se ne čita
    /// direktno po stranicama.
    /// </summary>
    Task<bool> JeDozvoljen(string kodModula);

    /// <summary>Svi kodovi modula dozvoljeni za trenutnu firmu.</summary>
    Task<HashSet<string>> DozvoljeniModuli();
}
