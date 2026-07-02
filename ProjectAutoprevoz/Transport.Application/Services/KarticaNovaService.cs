using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services;

public class KarticaNovaService : IKarticaNovaService
{
    // ── Konstante za tipDokumenta ───────────────────────────────
    public const string TipRacun             = "RACUN";
    public const string TipUplata            = "UPLATA";
    public const string TipIsplata           = "ISPLATA";
    public const string TipKnjiznoOdobrenje  = "KNJIZNO_ODOBRENJE";
    public const string TipKnjiznoZaduzenje  = "KNJIZNO_ZADUZENJE";
    public const string TipPocetno           = "POCETNO";

    // ── Konstante za partnerUloga ───────────────────────────────
    public const string UlogaKupac     = "KUPAC";
    public const string UlogaDobavljac = "DOBAVLJAC";

    private readonly TransportDbContext  _db;
    private readonly ITenantService      _tenant;
    private readonly ILogBrisanjaService _log;

    public KarticaNovaService(
        TransportDbContext  db,
        ITenantService      tenant,
        ILogBrisanjaService log)
    {
        _db     = db;
        _tenant = tenant;
        _log    = log;
    }

    // ── Matrica znakova ─────────────────────────────────────────
    // Saldo = Duguje - Potrazuje (uvek).
    // Preostalo = iznos za zaduženja (RACUN/POCETNO/KNJIZNO_ZADUZENJE),
    //             0 za plaćanja/odobrenja (UPLATA/ISPLATA/KNJIZNO_ODOBRENJE).
    private static (decimal duguje, decimal potrazuje, decimal saldo, decimal preostalo)
        ApplyMatrix(string tipDokumenta, string partnerUloga, decimal iznos)
    {
        // Zaduženja: red otvara obavezu, preostalo = iznos
        if (tipDokumenta is TipRacun or TipPocetno or TipKnjiznoZaduzenje)
        {
            if (partnerUloga == UlogaKupac)
                return (iznos, 0, iznos, iznos);       // kupac duguje nama
            else
                return (0, iznos, -iznos, iznos);      // mi dugujemo dobavljaču
        }

        // Plaćanja: red zatvara obavezu, preostalo = 0
        if (tipDokumenta == TipUplata)                 // novac primili od kupca
            return (0, iznos, -iznos, 0);

        if (tipDokumenta == TipIsplata)                // novac platili dobavljaču
            return (iznos, 0, iznos, 0);

        if (tipDokumenta == TipKnjiznoOdobrenje)
        {
            if (partnerUloga == UlogaKupac)
                return (0, iznos, -iznos, 0);          // smanjuje potraživanje od kupca
            else
                return (iznos, 0, iznos, 0);           // smanjuje obavezu prema dobavljaču
        }

        throw new InvalidOperationException(
            $"Nepoznata kombinacija tipDokumenta='{tipDokumenta}', partnerUloga='{partnerUloga}'.");
    }

    // ── Audit polja ─────────────────────────────────────────────
    private void PopuniAuditUnos(KarticaNova k)
    {
        k.Uneo       = _tenant.GetIdKorisnika();
        k.DatumUnosa = DateTime.Now;
    }

    private void PopuniAuditIzmena(KarticaNova k)
    {
        k.Izmenio    = _tenant.GetIdKorisnika();
        k.DatumIzmene = DateTime.Now;
    }

    // ── Pomoćna: mapiranje TipProdaje računa na ulogu/valutu ────
    private static (string uloga, string valuta) MapirajRacun(string? tipProdaje) =>
        tipProdaje switch
        {
            "IZLAZ"     or "IZLAZ_BP" => (UlogaKupac,     "RSD"),
            "INOSTRANI"               => (UlogaKupac,     "EUR"),
            "ULAZ"                    => (UlogaDobavljac,  "RSD"),
            "INO_ULAZ"                => (UlogaDobavljac,  "EUR"),
            _                         => (UlogaKupac,     "RSD")
        };

    // ── DodajStavku (standalone, sa SaveChangesAsync) ───────────
    public async Task<int> DodajStavku(KarticaNova stavka, decimal iznos)
    {
        if (iznos <= 0)
            throw new ArgumentException("Iznos mora biti pozitivan.", nameof(iznos));

        var (dug, pot, sal, pre) = ApplyMatrix(stavka.TipDokumenta, stavka.PartnerUloga, iznos);
        stavka.Duguje    = dug;
        stavka.Potrazuje = pot;
        stavka.Saldo     = sal;
        stavka.Preostalo = pre;
        stavka.Izmiren   = false;

        PopuniAuditUnos(stavka);

        _db.KarticeNova.Add(stavka);
        await _db.SaveChangesAsync();
        return stavka.Id;
    }

    // ── DodajUplatuVezanu (Case A: nova uplata + link za zaduženje) ─────
    public async Task<int> DodajUplatuVezanu(KarticaNova stavka, decimal iznos, int idZaduzenja)
    {
        if (iznos <= 0)
            throw new ArgumentException("Iznos mora biti pozitivan.", nameof(iznos));

        var zaduzenje = await _db.KarticeNova.FindAsync(idZaduzenja)
            ?? throw new InvalidOperationException($"Zaduženje {idZaduzenja} nije pronađeno.");

        var p = zaduzenje.Preostalo;
        var u = iznos;
        var iznosVezani = Math.Min(u, p);

        // Uplata se vezuje za zaduženje na iznos min(U, P)
        var (dug, pot, sal, _) = ApplyMatrix(stavka.TipDokumenta, stavka.PartnerUloga, iznosVezani);
        stavka.Duguje        = dug;
        stavka.Potrazuje     = pot;
        stavka.Saldo         = sal;
        stavka.Preostalo     = 0;
        stavka.Izmiren       = false;
        stavka.IdStavkeVeza  = idZaduzenja;
        stavka.BrojDokumenta = zaduzenje.BrojDokumenta;
        PopuniAuditUnos(stavka);
        _db.KarticeNova.Add(stavka);

        // Ažuriraj preostalo zaduženja
        zaduzenje.Preostalo -= iznosVezani;
        zaduzenje.Izmiren    = zaduzenje.Preostalo <= 0;
        PopuniAuditIzmena(zaduzenje);

        // U > P → cepanje: INSERT NERASPOREDJENO za ostatak (U-P)
        if (u > p)
            _db.KarticeNova.Add(KreirajNerasporedjenu(stavka, u - p));

        await _db.SaveChangesAsync();
        return stavka.Id;
    }

    // ── VezujNerasporedjenu (Case B: link postojeće neraspoređene) ──────
    public async Task VezujNerasporedjenu(int idNerasporedjene, int idZaduzenja)
    {
        var uplata = await _db.KarticeNova.FindAsync(idNerasporedjene)
            ?? throw new InvalidOperationException($"Uplata {idNerasporedjene} nije pronađena.");
        var zaduzenje = await _db.KarticeNova.FindAsync(idZaduzenja)
            ?? throw new InvalidOperationException($"Zaduženje {idZaduzenja} nije pronađeno.");

        // N = iznos neraspoređene (Potrazuje za UPLATA, Duguje za ISPLATA)
        var n = Math.Max(uplata.Potrazuje, uplata.Duguje);
        var p = zaduzenje.Preostalo;
        var iznosVezani = Math.Min(n, p);

        if (n <= p)
        {
            // Cela neraspoređena se vezuje — samo promena idStavkeVeza i broja
            uplata.IdStavkeVeza  = idZaduzenja;
            uplata.BrojDokumenta = zaduzenje.BrojDokumenta;
            PopuniAuditIzmena(uplata);
        }
        else
        {
            // N > P → cepanje: neraspoređena pada na P (vezana), ostatak → nova NERASPOREDJENO
            var (dug, pot, sal, _) = ApplyMatrix(uplata.TipDokumenta, uplata.PartnerUloga, p);
            uplata.Duguje        = dug;
            uplata.Potrazuje     = pot;
            uplata.Saldo         = sal;
            uplata.IdStavkeVeza  = idZaduzenja;
            uplata.BrojDokumenta = zaduzenje.BrojDokumenta;
            PopuniAuditIzmena(uplata);

            _db.KarticeNova.Add(KreirajNerasporedjenu(uplata, n - p));
        }

        zaduzenje.Preostalo -= iznosVezani;
        zaduzenje.Izmiren    = zaduzenje.Preostalo <= 0;
        PopuniAuditIzmena(zaduzenje);

        await _db.SaveChangesAsync();
    }

    // ── Pomoćna: kreira novu NERASPOREDJENO stavku iz izvora ────────────
    private KarticaNova KreirajNerasporedjenu(KarticaNova izvor, decimal iznos)
    {
        var (dug, pot, sal, _) = ApplyMatrix(izvor.TipDokumenta, izvor.PartnerUloga, iznos);
        return new KarticaNova
        {
            IdPartnera     = izvor.IdPartnera,
            PIB            = izvor.PIB,
            NazivPartnera  = izvor.NazivPartnera,
            PartnerUloga   = izvor.PartnerUloga,
            TipDokumenta   = izvor.TipDokumenta,
            BrojDokumenta  = "NERASPOREDJENO",
            IdRacun        = null,
            IdStavkeVeza   = null,
            Duguje         = dug,
            Potrazuje      = pot,
            Saldo          = sal,
            Preostalo      = 0,
            Izmiren        = false,
            Valuta         = izvor.Valuta,
            Kurs           = izvor.Kurs,
            DatumDokumenta = izvor.DatumDokumenta,
            DatumPrometa   = izvor.DatumPrometa,
            DatumValute    = izvor.DatumValute,
            Opis           = izvor.Opis,
            Izvod          = izvor.Izvod,
            Uneo           = _tenant.GetIdKorisnika(),
            DatumUnosa     = DateTime.Now
        };
    }

    // ── UpisiIzRacuna (stage-only, bez SaveChangesAsync) ────────
    public async Task UpisiIzRacuna(Racun racun)
    {
        var iznos  = racun.SumaRacuna ?? 0;
        var (uloga, valuta) = MapirajRacun(racun.TipProdaje);
        var idRacun = racun.Broj;

        var (dug, pot, sal, pre) = ApplyMatrix(TipRacun, uloga, iznos);

        var postojeca = await _db.KarticeNova
            .FirstOrDefaultAsync(k => k.IdRacun == idRacun && k.TipDokumenta == TipRacun);

        if (postojeca is null)
        {
            _db.KarticeNova.Add(new KarticaNova
            {
                IdPartnera     = racun.IdPartnera,
                PIB            = racun.PIB,
                NazivPartnera  = racun.Naziv,
                PartnerUloga   = uloga,
                TipDokumenta   = TipRacun,
                BrojDokumenta  = racun.BrojRacuna,
                IdRacun        = idRacun,
                Duguje         = dug,
                Potrazuje      = pot,
                Saldo          = sal,
                Preostalo      = pre,
                Izmiren        = false,
                Valuta         = valuta,
                Kurs           = valuta == "EUR" ? racun.Kurs : null,
                DatumDokumenta = racun.DatumRacuna,
                DatumPrometa   = racun.DatumPrometaDo,
                DatumValute    = racun.DatumValute,
                Uneo           = _tenant.GetIdKorisnika(),
                DatumUnosa     = DateTime.Now
            });
        }
        else
        {
            // Ažuriranje: iznos ili tip računa se mogao promeniti.
            // Preostalo se skalira: ako je preostalo < dug (delimično plaćen),
            // smanjujemo proporcionalno — ali za sada čuvamo razliku.
            var staroDuguje = postojeca.Duguje != 0 ? postojeca.Duguje : postojeca.Potrazuje;
            var rasporedeno = staroDuguje - postojeca.Preostalo; // koliko je već zatvoreno
            var novoPreostalo = Math.Max(0, iznos - rasporedeno);

            postojeca.IdPartnera    = racun.IdPartnera;
            postojeca.PIB           = racun.PIB;
            postojeca.NazivPartnera = racun.Naziv;
            postojeca.PartnerUloga  = uloga;
            postojeca.BrojDokumenta = racun.BrojRacuna;
            postojeca.Duguje        = dug;
            postojeca.Potrazuje     = pot;
            postojeca.Saldo         = sal;
            postojeca.Preostalo     = novoPreostalo;
            postojeca.Izmiren       = novoPreostalo <= 0;
            postojeca.Valuta        = valuta;
            postojeca.Kurs          = valuta == "EUR" ? racun.Kurs : null;
            postojeca.DatumDokumenta = racun.DatumRacuna;
            postojeca.DatumPrometa  = racun.DatumPrometaDo;
            postojeca.DatumValute   = racun.DatumValute;

            PopuniAuditIzmena(postojeca);
        }
    }

    // ── ObrisiIzRacuna (stage-only, bez SaveChangesAsync) ───────
    public async Task ObrisiIzRacuna(int idRacun)
    {
        var stavke = await _db.KarticeNova
            .Where(k => k.IdRacun == idRacun)
            .ToListAsync();

        foreach (var s in stavke)
        {
            var valuta = s.Valuta;
            var iznos  = s.Duguje != 0 ? s.Duguje : s.Potrazuje;
            await _log.Zabelezi("KARTICE_NOVA",
                $"Obrisana stavka {s.TipDokumenta} {iznos:N2} {valuta}, " +
                $"partner {s.NazivPartnera}, račun {s.BrojDokumenta}");
        }

        _db.KarticeNova.RemoveRange(stavke);
    }

    // ── ObrisiUplatuNova ─────────────────────────────────────────
    public async Task ObrisiUplatuNova(int idUplate)
    {
        var uplata = await _db.KarticeNova.FindAsync(idUplate)
            ?? throw new InvalidOperationException($"Stavka {idUplate} nije pronađena.");

        // Ako je vezana za zaduženje → vrati preostalo pre brisanja
        if (uplata.IdStavkeVeza.HasValue)
        {
            var zaduzenje = await _db.KarticeNova.FindAsync(uplata.IdStavkeVeza.Value);
            if (zaduzenje is not null)
            {
                var iznos       = Math.Max(uplata.Potrazuje, uplata.Duguje);
                var maxPreostalo = Math.Max(zaduzenje.Duguje, zaduzenje.Potrazuje);
                zaduzenje.Preostalo = Math.Min(zaduzenje.Preostalo + iznos, maxPreostalo);
                zaduzenje.Izmiren   = zaduzenje.Preostalo <= 0;
                PopuniAuditIzmena(zaduzenje);
            }
        }

        var iznos2 = Math.Max(uplata.Potrazuje, uplata.Duguje);
        await _log.Zabelezi("KARTICE_NOVA",
            $"Brisana stavka {uplata.TipDokumenta} {iznos2:N2} {uplata.Valuta}, " +
            $"partner {uplata.NazivPartnera}");

        _db.KarticeNova.Remove(uplata);
        await _db.SaveChangesAsync();
    }

    // ── ProveriUplateZaRacun ──────────────────────────────────────
    public async Task<string?> ProveriUplateZaRacun(int idRacun)
    {
        var stavka = await _db.KarticeNova
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.IdRacun == idRacun && k.TipDokumenta == TipRacun);

        if (stavka is null) return null; // Nema stavke u novoj tabeli → ok

        var imaUplate = await _db.KarticeNova
            .AnyAsync(k => k.IdStavkeVeza == stavka.Id);

        return imaUplate
            ? $"Račun {stavka.BrojDokumenta} ima vezane uplate u novom modelu. " +
              "Prvo odveži ili obriši uplate, pa zatim obriši račun."
            : null;
    }

    // ── OdveziUplatu ─────────────────────────────────────────────
    public async Task OdveziUplatu(int idUplate)
    {
        var uplata = await _db.KarticeNova.FindAsync(idUplate)
            ?? throw new InvalidOperationException($"Uplata {idUplate} nije pronađena.");

        if (!uplata.IdStavkeVeza.HasValue)
            throw new InvalidOperationException("Uplata nije vezana ni za jedno zaduženje.");

        var zaduzenje = await _db.KarticeNova.FindAsync(uplata.IdStavkeVeza.Value)
            ?? throw new InvalidOperationException($"Zaduženje {uplata.IdStavkeVeza} nije pronađeno.");

        var iznos         = Math.Max(uplata.Potrazuje, uplata.Duguje);
        var maxPreostalo  = Math.Max(zaduzenje.Duguje, zaduzenje.Potrazuje);

        await _log.Zabelezi("KARTICE_NOVA",
            $"Odvezana uplata {iznos:N2} {uplata.Valuta} sa zaduženja {zaduzenje.BrojDokumenta}, " +
            $"partner {uplata.NazivPartnera}");

        zaduzenje.Preostalo = Math.Min(zaduzenje.Preostalo + iznos, maxPreostalo);
        zaduzenje.Izmiren   = zaduzenje.Preostalo <= 0;
        PopuniAuditIzmena(zaduzenje);

        uplata.IdStavkeVeza  = null;
        uplata.BrojDokumenta = "NERASPOREDJENO";
        PopuniAuditIzmena(uplata);

        await _db.SaveChangesAsync();
    }

    // ── ObrisiRucniRacunStavku ───────────────────────────────────
    public async Task<string?> ObrisiRucniRacunStavku(int idStavke)
    {
        var stavka = await _db.KarticeNova.FindAsync(idStavke)
            ?? throw new InvalidOperationException($"Stavka {idStavke} nije pronađena.");

        if (stavka.TipDokumenta != TipRacun || stavka.IdRacun is not null)
            throw new InvalidOperationException("Metoda je samo za ručno unete RACUN stavke (bez FK na tbl_racuni).");

        var imaVezanih = await _db.KarticeNova.AnyAsync(k => k.IdStavkeVeza == idStavke);
        if (imaVezanih)
            return $"Stavka {stavka.BrojDokumenta} ima vezane uplate. Prvo odveži ili obriši uplate, pa zatim obriši ručni račun.";

        var iznos = stavka.Duguje != 0 ? stavka.Duguje : stavka.Potrazuje;
        await _log.Zabelezi("KARTICE_NOVA",
            $"Obrisana ručna RACUN stavka {iznos:N2} {stavka.Valuta}, " +
            $"partner {stavka.NazivPartnera}, dokument {stavka.BrojDokumenta}");

        _db.KarticeNova.Remove(stavka);
        await _db.SaveChangesAsync();
        return null;
    }

    // ── SaldoPartnera ────────────────────────────────────────────
    public async Task<decimal> SaldoPartnera(int idPartnera, string valuta, string uloga)
    {
        return await _db.KarticeNova
            .Where(k => k.IdPartnera == idPartnera
                     && k.Valuta == valuta
                     && k.PartnerUloga == uloga)
            .SumAsync(k => (decimal?)k.Saldo) ?? 0m;
    }
}
