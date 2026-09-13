namespace Transport.Domain.Helpers;

/// <summary>
/// Čitljiva generisana lozinka oblika "PrvaRec-4cifre" (npr. Prevoz-4821, Marko-7213).
/// JEDAN generator — koristi ga ProvisioningService (nova firma, seed = naziv firme) i
/// WebKorisnikDialog (reset lozinke, seed = ime korisnika) — da se ponašanje ne razdvoji.
/// </summary>
public static class LozinkaHelper
{
    public static string Generisi(string? tekst)
    {
        var prvaRec = (tekst ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Firma";
        prvaRec = new string(prvaRec.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(prvaRec)) prvaRec = "Firma";
        prvaRec = char.ToUpperInvariant(prvaRec[0]) + (prvaRec.Length > 1 ? prvaRec[1..] : "");

        var cifre = Random.Shared.Next(1000, 10000);
        return $"{prvaRec}-{cifre}";
    }
}
