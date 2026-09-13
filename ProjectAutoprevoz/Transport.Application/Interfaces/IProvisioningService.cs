namespace Transport.Application.Interfaces;

/// <summary>Ulazni podaci za kreiranje nove WEB firme (tbl_web_licence + tbl_web_clanstvo).</summary>
public record NovaWebFirmaZahtev(
    string Zemlja,
    string KodDrzave,
    string Pib,
    string Naziv,
    string? MaticniBroj,
    string? Adresa,
    string? Mesto,
    string? PostanskiBroj,
    string TipPrograma,
    bool ModulTure,
    bool ModulRadniNalozi,
    bool ModulLager,
    string TipLicence,
    DateTime? DatumOd,
    DateTime? DatumDo,
    int? MaxKorisnika,
    string ImeVlasnika,
    string EmailVlasnika,
    string? Telefon,
    bool BazaVecPostoji,
    string? ConnectionString,
    // Kad bazaVecPostoji i tbl_Podaci u toj bazi nema PIB (ne može se provjeriti
    // poklapanje sa formom) — samo tada ova potvrda propušta nastavak. NE važi za
    // stvaran PIB mismatch (tbl_Podaci ima PIB koji se ne poklapa) — to je uvijek
    // hard stop, bez izuzetka.
    bool PotvrdjenoNepoklapanje = false,
    // Ključ iz appsettings.json "SqlServeri" sekcije — koji server dobija NOVU bazu.
    // Relevantno samo kad BazaVecPostoji=false. Null/prazno -> prvi server u sekciji
    // (ili stari fallback ako sekcija ne postoji, vidi KreirajWebFirmuAsync).
    string? SqlServer = null);

/// <summary>
/// Rezultat kreiranja nove web firme. GenerisanaLozinka je NULL ako je korisnik
/// (po emailu) već postojao — u tom slučaju KorisnikVecPostojao je true i lozinka
/// se ne dira. Plain-text lozinka se NIGDE ne čuva osim u ovom povratnom objektu.
///
/// PotrebnaPotvrda=true znači: bazaVecPostoji, ali tbl_Podaci u toj bazi nema PIB,
/// pa se ne može automatski provjeriti da je to tačna firma — PronadjenNaziv je
/// naziv pronađen u toj bazi (može biti null/prazno). Ništa nije upisano; UI treba
/// da pita korisnika i, na potvrdu, ponovi poziv sa PotvrdjenoNepoklapanje=true.
/// </summary>
public record NovaWebFirmaRezultat(
    bool Uspeh, string Poruka, int? IdWebLicence, string? ImeBaze,
    string? Email, string? GenerisanaLozinka, bool KorisnikVecPostojao,
    bool PotrebnaPotvrda = false, string? PronadjenNaziv = null);

public interface IProvisioningService
{
    /// <summary>
    /// Kreira novu WEB firmu — bazu (ili koristi postojeću), tbl_Podaci/tbl_zaposleni
    /// u klijentskoj bazi, i tbl_web_licence/tbl_web_korisnici/tbl_web_clanstvo u master
    /// bazi. Logička transakcija — rollback (DROP DATABASE ako je kreirana u ovom pozivu,
    /// poništaj upise u masteru) pri bilo kojoj grešci. Ako bazaVecPostoji, baza se NIKAD
    /// ne dira (ni pri uspehu ni pri neuspehu).
    /// </summary>
    Task<NovaWebFirmaRezultat> KreirajWebFirmuAsync(NovaWebFirmaZahtev zahtev);
}
