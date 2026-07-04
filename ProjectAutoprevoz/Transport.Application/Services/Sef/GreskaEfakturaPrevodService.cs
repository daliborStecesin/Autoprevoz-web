using System.Text.Json;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Prevodi SEF ErrorCode vrednosti na srpski, iz statičkog resurs fajla
/// (Resources/greskeEFakture.json — niz sa jednim objektom ErrorCode→prevod).
/// Fajl se učitava JEDNOM u konstruktoru (Singleton lifetime) i drži u memoriji.
/// </summary>
public class GreskaEfakturaPrevodService : IGreskaEfakturaPrevodService
{
    private readonly Dictionary<string, string> _prevodi;

    public GreskaEfakturaPrevodService()
    {
        _prevodi = UcitajPrevode();
    }

    private static Dictionary<string, string> UcitajPrevode()
    {
        try
        {
            var putanja = Path.Combine(AppContext.BaseDirectory, "Resources", "greskeEFakture.json");
            if (!File.Exists(putanja)) return new Dictionary<string, string>();

            var json = File.ReadAllText(putanja);
            var niz = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);

            return niz is { Count: > 0 }
                ? niz[0]
                : new Dictionary<string, string>();
        }
        catch
        {
            // Isti fallback duh kao stari greskaPrevod — nikad ne sme srušiti pozivaoca
            // zbog nedostupnog/neispravnog resurs fajla.
            return new Dictionary<string, string>();
        }
    }

    public string Prevedi(string? errorCode, string? fallbackMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorCode) && _prevodi.TryGetValue(errorCode, out var prevod))
            return prevod;

        if (!string.IsNullOrWhiteSpace(fallbackMessage))
            return fallbackMessage;

        return errorCode ?? "";
    }
}
