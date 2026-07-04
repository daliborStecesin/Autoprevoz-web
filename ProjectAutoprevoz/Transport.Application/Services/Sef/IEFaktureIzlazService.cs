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
    /// invoiceID), za svaki nov upisuje red (XML parsiranje + status). Vraća broj
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

    /// Ceo envelope XML sa SEF-a, bez izdvajanja (isto kao Ulazne e-fakture).
    Task<string?> PreuzmiXmlAsync(string invoiceId);
}
