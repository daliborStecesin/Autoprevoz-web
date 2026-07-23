namespace Transport.Application.Interfaces;

public interface ITenantService
{
    void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija, int idLicence = 0);
    string GetConnectionString();
    string GetNazivFirme();
    int GetPrivilegija();
    int GetIdKorisnika();
    int GetIdLicence();
    string GetImeKorisnika();
    bool GetTransportModulAktivan();
    bool GetEFakturaAktivna();
    bool JeImpersonacija();
    bool IsAuthenticated();
    void Logout();
}
