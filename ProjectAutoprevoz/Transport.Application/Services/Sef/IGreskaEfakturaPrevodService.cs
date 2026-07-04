namespace Transport.Application.Services.Sef;

public interface IGreskaEfakturaPrevodService
{
    /// Prevodi SEF ErrorCode na srpski. Ako errorCode ne postoji u rečniku, vraća
    /// fallbackMessage (ako nije prazan), inače sam errorCode. Nikad ne baca exception.
    string Prevedi(string? errorCode, string? fallbackMessage);
}
