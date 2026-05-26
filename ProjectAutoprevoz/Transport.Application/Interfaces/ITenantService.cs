namespace Transport.Application.Interfaces;

public interface ITenantService
{
    void SetTenant(string connectionString, string nazivFirme, int idKorisnika, int privilegija);
    string GetConnectionString();
    string GetNazivFirme();
    int GetPrivilegija();
    int GetIdKorisnika();
    bool IsAuthenticated();
    void Logout();
}
