namespace Transport.Application.Interfaces;

public interface ITenantService
{
    void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija);
    string GetConnectionString();
    string GetNazivFirme();
    int GetPrivilegija();
    int GetIdKorisnika();
    int GetIdFirme();
    string GetImeKorisnika();
    bool GetTransportModulAktivan();
    bool GetEFakturaAktivna();
    bool JeImpersonacija();
    bool IsAuthenticated();
    Task<int> RolaTrenutneFirme();
    Task<bool> JeVlasnikTrenutneFirme();
    Task<bool> JeSamoCitanje();
    Task<DateTime?> DatumDoTrenutneFirme();
    void Logout();
}
