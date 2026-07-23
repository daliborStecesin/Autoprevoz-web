namespace Transport.Application.Interfaces;

/// <summary>
/// Rezultat kreiranja nove klijentske firme (baze + licence + prvog web korisnika).
/// </summary>
public record ProvisioningRezultat(bool Uspeh, string Poruka, int? IdLicence, string NazivBaze);

/// <summary>
/// Kreira novu klijentsku firmu: bazu rs{PIB} (iz 01_CREATE_kasa_template.sql),
/// red u master tbl_licence, prvog zaposlenog (tbl_imenik) i prvog web korisnika
/// (tbl_web_korisnici, Privilegija=1). Faza 1 — poziva se isključivo iz super
/// admin panela.
/// </summary>
public interface IProvisioningService
{
    Task<ProvisioningRezultat> KreirajFirmuAsync(
        string pib, string nazivFirme, string ime, string prezime, string email, string lozinka);
}
