using Transport.Domain.Entities;

namespace Transport.Application.Interfaces;

/// <summary>
/// Servis za upis stavki u tbl_KarticaNova (novi finansijski model).
/// Matrica znakova (duguje/potrazuje/saldo) je enkapsulirana ovde —
/// pozivajući kod prosleđuje samo iznos (uvek pozitivan).
///
/// STAGING vs SaveChanges:
///   UpisiIzRacuna / ObrisiIzRacuna — stage-uju promene, ne pozivaju SaveChangesAsync
///   (pozivaju se iz Fakture transakcije, SaveChanges radi pozivajuća strana).
///   DodajStavku — poziva SaveChangesAsync i vraća novi Id
///   (za standalone upis van Fakture transakcije).
/// </summary>
public interface IKarticaNovaService
{
    /// <summary>
    /// Upiše novu stavku u karticu (standalone — poziva SaveChangesAsync, vraća Id).
    /// Iznos mora biti pozitivan. Duguje/Potrazuje/Saldo/Preostalo se računaju iz matrice.
    /// </summary>
    Task<int> DodajStavku(KarticaNova stavka, decimal iznos);

    /// <summary>
    /// Upiše ili ažurira stavku tipa RACUN za dati račun (stage-only, bez SaveChangesAsync).
    /// Poziva se iz Fakture transakcije zajedno sa SaveChangesAsync za račun.
    /// </summary>
    Task UpisiIzRacuna(Racun racun);

    /// <summary>
    /// Fizički briše stavke za dati idRacun + stage-uje log (bez SaveChangesAsync).
    /// Poziva se iz Fakture transakcije.
    /// </summary>
    Task ObrisiIzRacuna(int idRacun);

    /// <summary>
    /// Upiše novu uplatu/isplatu vezanu za konkretno zaduženje (Case A: nova uplata → link).
    /// Algoritam U-vs-P: ako uplata > preostalo zaduženja, cepanje → INSERT NERASPOREDJENO za ostatak.
    /// Poziva SaveChangesAsync.
    /// </summary>
    Task<int> DodajUplatuVezanu(KarticaNova stavka, decimal iznos, int idZaduzenja);

    /// <summary>
    /// Vezuje postojeću NERASPOREDJENU uplatu/isplatu za zaduženje (Case B: link postojeće).
    /// Algoritam N-vs-P: ako neraspoređena > preostalo, cepanje → INSERT NERASPOREDJENO za ostatak.
    /// Saldo partnera ostaje isti (samo premeštanje). Poziva SaveChangesAsync.
    /// </summary>
    Task VezujNerasporedjenu(int idNerasporedjene, int idZaduzenja);

    /// <summary>
    /// Briše ručno unetu uplatu/isplatu/pocetno iz kartice.
    /// Ako je uplata vezana (IdStavkeVeza != null): vraća preostalo zaduženju, izmiren recalc.
    /// Loguje brisanje + fizički briše. Poziva SaveChangesAsync.
    /// </summary>
    Task ObrisiUplatuNova(int idUplate);

    /// <summary>
    /// Provera blokade pre brisanja računa: da li postoje vezane uplate na RACUN stavci u KarticaNova.
    /// Vraća null ako je ok za brisanje, ili poruku greške ako je blokirano.
    /// </summary>
    Task<string?> ProveriUplateZaRacun(int idRacun);

    /// <summary>
    /// Odvezuje uplatu/isplatu od zaduženja — vraća je u NERASPOREDJENO, preostalo zaduženja raste.
    /// Saldo partnera ostaje isti. Poziva SaveChangesAsync.
    /// </summary>
    Task OdveziUplatu(int idUplate);

    /// <summary>
    /// Briše ručno unetu RACUN stavku (idRacun==null) ili KNJIZNO_ZADUZENJE stavku.
    /// Guard: vraća poruku greške ako ima vezanih uplata (blokira brisanje).
    /// Ako nema → loguje + fizički briše + SaveChangesAsync. Vraća null ako ok.
    /// </summary>
    Task<string?> ObrisiRucniRacunStavku(int idStavke);

    /// <summary>
    /// Vraća SUM(saldo) za partnera, valutu i ulogu (KUPAC/DOBAVLJAC).
    /// Pozitivan saldo: kupac duguje ili dobavljač je preplaćen.
    /// Negativan saldo: kupac je preplaćen ili dobavljaču dugujemo.
    /// </summary>
    Task<decimal> SaldoPartnera(int idPartnera, string valuta, string uloga);
}
