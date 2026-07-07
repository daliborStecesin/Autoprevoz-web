using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;

namespace Transport.Application.Services.Sef;

public interface IEFaktureUlazService
{
    /// Čitanje iz lokalne baze (tbl_eFakturaUlaz), bez poziva ka SEF-u.
    /// Svi parametri opciono filtriraju; ako su svi null/default, vraća sve.
    Task<List<EFakturaUlaz>> GetListaAsync(
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

    /// Prihvata/odbija ulaznu fakturu na SEF-u; na uspeh osvežava lokalni status.
    Task<(bool uspesno, string poruka)> PrihvatiOdbijAsync(string invoiceId, bool prihvati, string komentar);

    /// PDF je ugrađen u envelope XML (env:DocumentHeader > env:DocumentPdf, Base64).
    /// Vraća null ako element ne postoji/nema sadržaj (PDF nije generisan za dokument).
    Task<byte[]?> PreuzmiPdfAsync(string invoiceId);

    /// Ceo envelope XML sa SEF-a, bez izdvajanja (isto kao stari btnPreuzmiXML_Click).
    Task<string?> PreuzmiXmlAsync(string invoiceId);

    /// Učitava prateće dokumente (priloge) iz envelope XML-a (cac:AdditionalDocumentReference).
    /// Preskače reference bez ugrađenog Base64 sadržaja (npr. samo broj narudžbenice).
    /// Vraća praznu listu ako dokument nema priloga.
    Task<List<PrateciDokument>> UcitajPrateceDokumenteAsync(string invoiceId);
}
