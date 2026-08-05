using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface IEFaktureIzlazService
{
    /// Čitanje iz lokalne baze (tbl_eInvoice), bez poziva ka SEF-u.
    /// Svi parametri opciono filtriraju; ako su svi null/default, vraća sve.
    Task<List<EInvoice>> GetListaAsync(
        string? nazivIliPibFilter,
        string? brojDokumentaFilter,
        string? tipDokumentaFilter,
        string? statusFilter,
        string tipDatuma,
        DateTime? datumOd,
        DateTime? datumDo);

    /// Povlači listu ID-jeva sa SEF-a za dati period, preskače već upisane (dedupe po
    /// salesInvoiceID), za svaki nov upisuje red (XML parsiranje + status). Vraća broj
    /// upisanih redova i listu PIB-ova partnera koji ne postoje lokalno (za info, bez insert-a).
    Task<(int brojNovih, List<string> upozorenjaPartner)> SinhronizujAsync(DateTime datumOd, DateTime datumDo);

    /// Osvežava statusDokumenta za postojeći red sa SEF-a. Vraća novi (prevedeni)
    /// status, ili null ako red ne postoji lokalno.
    Task<string?> OsveziStatusAsync(string invoiceId);

    /// Vraća sirov Comment sa SEF-a (za "live" osvežavanje komentara odbijanja u Fazi C).
    Task<string?> UzmiKomentarOdbijanjaAsync(string invoiceId);

    /// Stornira izlaznu e-fakturu na SEF-u (POST sales-invoice/storno); na uspeh osvežava lokalni status.
    Task<(bool uspesno, string poruka)> StornoAsync(string invoiceId, string komentar);

    /// Otkazuje slanje izlazne e-fakture na SEF-u (POST sales-invoice/cancel); na uspeh osvežava lokalni status.
    Task<(bool uspesno, string poruka)> OtkaziAsync(string invoiceId, string komentar);

    /// PDF je ugrađen u envelope XML (env:DocumentHeader > env:DocumentPdf, Base64).
    /// Vraća null ako element ne postoji/nema sadržaj (PDF nije generisan za dokument).
    Task<byte[]?> PreuzmiPdfAsync(string invoiceId);

    /// Prošireni PDF preko posebnog SEF endpointa (sales-invoice/pdf, binarni odgovor).
    /// Vraća null ako SEF umesto PDF-a vrati poruku da se dokument još generiše.
    Task<byte[]?> PreuzmiProsireniPdfAsync(string invoiceId);

    /// PDF istorije statusa dokumenta (sales-invoice/status-history/{invoiceId}/pdf).
    /// Vraća null ako SEF umesto PDF-a vrati poruku da se dokument još generiše.
    Task<byte[]?> PreuzmiStatusHistoryPdfAsync(string invoiceId);

    /// Ceo envelope XML sa SEF-a, bez izdvajanja (isto kao Ulazne e-fakture).
    Task<string?> PreuzmiXmlAsync(string invoiceId);

    /// Tiho briše sa SEF-a dokumente u statusu Draft i New za dati period (1:1 prevod
    /// starog btnBrisiPripremu_Click, bez MessageBox-a — poziva se automatski pri
    /// otvaranju stranice). Vraća ukupan broj obrisanih dokumenata. Ne baca izuzetak —
    /// SEF nedostupnost ili greška po statusu se tiho gutaju (lista se svejedno prikaže).
    Task<int> ObrisiDokumenteUPripremiAsync(DateTime datumOd, DateTime datumDo);

    /// Učitava prateće dokumente (priloge) iz envelope XML-a (cac:AdditionalDocumentReference).
    /// Preskače reference bez ugrađenog Base64 sadržaja (npr. samo broj narudžbenice).
    /// Vraća praznu listu ako dokument nema priloga.
    Task<List<PrateciDokument>> UcitajPrateceDokumenteAsync(string invoiceId);

    /// Šalje gotov UBL XML na SEF (POST sales-invoice/ubl, requestId generiše servis).
    /// TEK NA USPEH upisuje glavu (tbl_eInvoice) + stavke (tbl_lineItem) u transakciji —
    /// na grešku se ništa ne upisuje. Vraća SalesInvoiceId, idEfakture (PK novog reda)
    /// i statusDokumenta na uspeh; prevedenu (ili sirovu) SEF grešku na neuspeh.
    Task<(bool uspesno, string poruka, string? salesInvoiceId, int? idEfakture, string? statusDokumenta)>
        PosaljiUblAsync(string xml, bool sendToCir, EFakturaUblInput input, EFakturaSlanjeKontekst kontekst);

    /// Jednokratni backfill: za stare redove gde je invoiceIDint NULL a salesInvoiceID
    /// se parsira u broj, popunjava invoiceIDint (kolona već postoji, samo je do sad
    /// ostajala prazna). Bezbedno za pozivanje uvek — bez kandidata je no-op. Vraća broj
    /// ažuriranih redova.
    Task<int> PopuniInvoiceIdIntBackfillAsync();
}
