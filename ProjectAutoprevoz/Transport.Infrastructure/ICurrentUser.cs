namespace Transport.Infrastructure;

public interface ICurrentUser
{
    int GetIdKorisnika();

    // Read-only režim trenutne firme (SamoCitanje=1 ili licenca istekla) — TransportDbContext
    // pita ovo pre SaveChanges. Isti bridge-obrazac kao GetIdKorisnika() (Infrastructure ne
    // zavisi od Web/TenantService direktno).
    Task<bool> JeSamoCitanje();
}
