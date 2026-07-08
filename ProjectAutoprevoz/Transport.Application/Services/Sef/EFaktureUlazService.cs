using System.Globalization;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Transport.Application.Interfaces;
using Transport.Application.Services.Sef.Models;
using Transport.Domain.Entities;
using Transport.Infrastructure.Data;

namespace Transport.Application.Services.Sef;

/// <summary>
/// Ulazne e-fakture — lokalna lista (tbl_eFakturaUlaz) + SEF sinhronizacija
/// (purchase-invoice/ids, /xml, status, acceptRejectPurchaseInvoice).
/// PDF/XML export i integracija sa karticom dolaze u narednim fazama.
/// </summary>
public class EFaktureUlazService : IEFaktureUlazService
{
    private readonly TransportDbContext           _db;
    private readonly SefApiClient                 _api;
    private readonly IPartnerService              _partnerService;
    private readonly IGreskaEfakturaPrevodService _greskaPrevod;

    public EFaktureUlazService(
        TransportDbContext db, SefApiClient api, IPartnerService partnerService,
        IGreskaEfakturaPrevodService greskaPrevod)
    {
        _db             = db;
        _api            = api;
        _partnerService = partnerService;
        _greskaPrevod   = greskaPrevod;
    }

    private async Task<(string apiKey, string tipServera)> GetSettings()
    {
        var pod = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        return (pod?.sefApiKey ?? string.Empty, pod?.sefTipServera ?? "DEMO");
    }

    public async Task<List<EFakturaUlaz>> GetListaAsync(
        string? nazivIliPibFilter,
        string? brojDokumentaFilter,
        string? tipDokumentaFilter,
        string? statusFilter,
        string tipDatuma,
        DateTime? datumOd,
        DateTime? datumDo)
    {
        var q = _db.EFaktureUlaz.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nazivIliPibFilter))
            q = q.Where(x => (x.naziv != null && x.naziv.Contains(nazivIliPibFilter))
                           || (x.PIB   != null && x.PIB.Contains(nazivIliPibFilter)));

        if (!string.IsNullOrWhiteSpace(brojDokumentaFilter))
            q = q.Where(x => x.brojDokumenta != null && x.brojDokumenta.Contains(brojDokumentaFilter));

        if (!string.IsNullOrWhiteSpace(tipDokumentaFilter) && tipDokumentaFilter != "SVI TIPOVI")
        {
            // Stari upisi/stare baze koriste staru terminologiju (KNJIZNO ODOBRENJE/ZADUZENJE);
            // nove sinhronizacije pišu novu (DOKUMENT O SMANJENJU/POVECANJU). Dok se stari podaci
            // ne istope, filter mora prepoznati oba termina kao ekvivalentna.
            q = tipDokumentaFilter switch
            {
                "DOKUMENT O SMANJENJU" => q.Where(x => x.tipDokumenta == "DOKUMENT O SMANJENJU"
                                                     || x.tipDokumenta == "KNJIZNO ODOBRENJE"),
                "DOKUMENT O POVECANJU" => q.Where(x => x.tipDokumenta == "DOKUMENT O POVECANJU"
                                                     || x.tipDokumenta == "KNJIZNO ZADUZENJE"),
                _                      => q.Where(x => x.tipDokumenta == tipDokumentaFilter)
            };
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "SVI STATUSI")
            q = q.Where(x => x.statusDokumenta == statusFilter);

        var poPrometu = tipDatuma == "DATUM PROMETA";

        if (datumOd.HasValue)
        {
            var od = datumOd.Value.Date;
            q = poPrometu
                ? q.Where(x => x.accountingDateUtc >= od)
                : q.Where(x => x.invoiceDateUtc >= od);
        }

        if (datumDo.HasValue)
        {
            var doKraj = datumDo.Value.Date.AddDays(1).AddTicks(-1);
            q = poPrometu
                ? q.Where(x => x.accountingDateUtc <= doKraj)
                : q.Where(x => x.invoiceDateUtc <= doKraj);
        }

        // Sekundarni sort po invoiceIDint (pravi bigint, ne varchar) — u okviru istog
        // datuma noviji dokumenti (veći SEF ID) idu prvi. NULL invoiceIDint (stari
        // redovi pre backfill-a) automatski idu na kraj svog datuma — SQL Server
        // DESC sortira NULL kao najmanju vrednost, pa je poslednji u opadajućem redu.
        q = poPrometu
            ? q.OrderByDescending(x => x.accountingDateUtc).ThenByDescending(x => x.invoiceIDint)
            : q.OrderByDescending(x => x.invoiceDateUtc).ThenByDescending(x => x.invoiceIDint);

        return await q.ToListAsync();
    }

    // ── Backfill invoiceIDint — kolona već postoji, samo je stare redove (upisane
    // pre nego što je popunjavanje uvedeno) ostavljala prazne. Bira samo kandidate
    // (NULL invoiceIDint + invoiceID koji se parsira u broj), pa je posle prvog
    // uspešnog poziva trajno no-op (prazan WHERE rezultat). Izvor je invoiceID (ne
    // salesInvoiceID) — za Ulazne se salesInvoiceID nigde ne popunjava/koristi. ──
    public async Task<int> PopuniInvoiceIdIntBackfillAsync()
    {
        var kandidati = await _db.EFaktureUlaz
            .Where(x => x.invoiceIDint == null && x.invoiceID != null)
            .ToListAsync();

        var azurirano = 0;
        foreach (var red in kandidati)
        {
            if (long.TryParse(red.invoiceID, out var n))
            {
                red.invoiceIDint = n;
                azurirano++;
            }
        }

        if (azurirano > 0)
            await _db.SaveChangesAsync();

        return azurirano;
    }

    // ── Sinhronizacija (1:1 prevod btnUpisiSve_Click) ────────────────────────
    public async Task<(int brojNovih, List<string> upozorenjaPartner)> SinhronizujAsync(DateTime datumOd, DateTime datumDo)
    {
        var (apiKey, tipServera) = await GetSettings();
        var upozorenja = new List<string>();

        var endpointIds = $"purchase-invoice/ids?dateFrom={datumOd:yyyy-MM-dd}&dateTo={datumDo:yyyy-MM-dd}";
        // Isto kao stari PurchaseInvoiceIDs — ovaj endpoint zahteva POST (praznog tela),
        // ne GET, iako parametri opsega datuma idu kroz query string. GET vraća 405.
        var idsDto = await _api.PostEmptyAsync<PurchaseInvoiceIdsDto>(apiKey, tipServera, endpointIds);

        if (idsDto?.PurchaseInvoiceIds is null or { Count: 0 })
            return (0, upozorenja);

        // Dedupe u memoriji (ne preko .Contains() nad parametrizovanom listom u EF upitu) —
        // EF Core 8 prevodi takav .Contains() u OPENJSON(...) WITH (...), što ovaj SQL Server
        // (niži compatibility level) ne podržava ("Incorrect syntax near 'WITH'").
        var postojeciIds = (await _db.EFaktureUlaz
                .Where(x => x.invoiceID != null)
                .Select(x => x.invoiceID!)
                .ToListAsync())
            .ToHashSet();

        var sviPartneri = await _partnerService.GetSviPartneriAsync();

        var upisano = 0;

        foreach (var id in idsDto.PurchaseInvoiceIds)
        {
            var invoiceIdStr = id.ToString();
            if (postojeciIds.Contains(invoiceIdStr)) continue;

            var red = new EFakturaUlaz
            {
                invoiceID    = invoiceIdStr,
                invoiceIDint = id
            };

            try
            {
                var xml = await _api.GetStringAsync(apiKey, tipServera, $"purchase-invoice/xml?invoiceId={id}");
                PopuniIzXml(red, xml, sviPartneri, upozorenja);

                var statusDto = await _api.GetAsync<PurchaseInvoiceStatusDto>(
                    apiKey, tipServera, $"purchase-invoice?invoiceId={id}");
                red.statusDokumenta = PrevediStatus(statusDto?.Status);
            }
            catch
            {
                // Isto kao stari kod — pojedinačan XML/parsiranje koji padne ne prekida ceo
                // proces, red se upisuje sa onim što je uspelo do greške + fallback status.
                red.statusDokumenta ??= "Nepoznato";
            }

            _db.EFaktureUlaz.Add(red);
            postojeciIds.Add(invoiceIdStr);
            upisano++;
        }

        await _db.SaveChangesAsync();
        return (upisano, upozorenja);
    }

    private static void PopuniIzXml(EFakturaUlaz red, string xml, List<Partner> partneri, List<string> upozorenja)
    {
        var root = XDocument.Parse(xml).Root;

        // Ceo XML je zapravo ENVELOPE (isti env:DocumentHeader omotač kao za PDF) —
        // stvarni Invoice/CreditNote sadržaj je UGNJEŽDEN, ne na root nivou. Zato tražimo
        // rekurzivno kroz ceo dokument (isto kao stari GetElementsByTagName — uvek
        // globalna/rekurzivna pretraga, uzima prvi nađen element, bez obzira na dubinu).
        var brojRacuna = Vrednost(root, "ID");

        var issueDateStr    = Vrednost(root, "IssueDate");
        var dueDateStr      = Vrednost(root, "DueDate");
        var deliveryDateStr = Vrednost(root, "ActualDeliveryDate");

        var tipKod = Vrednost(root, "InvoiceTypeCode") ?? Vrednost(root, "CreditNoteTypeCode");

        var supplierParty = Podelement(root, "AccountingSupplierParty");
        var pib              = Vrednost(supplierParty, "EndpointID");
        var registrationName = Vrednost(supplierParty, "RegistrationName");

        var legalMonetaryTotal = Podelement(root, "LegalMonetaryTotal");
        var osnovica = ParsirajDecimal(Vrednost(legalMonetaryTotal, "TaxExclusiveAmount"));
        var vrednost = ParsirajDecimal(Vrednost(legalMonetaryTotal, "PayableAmount"));

        var taxTotal = Podelement(root, "TaxTotal");
        var pdvIznos = ParsirajDecimal(Vrednost(taxTotal, "TaxAmount"));

        var datumSlanja  = ParsirajDatum(issueDateStr);
        var datumValute  = ParsirajDatum(dueDateStr);
        var datumPrometa = ParsirajDatum(deliveryDateStr) ?? datumSlanja;

        string naziv;
        if (!string.IsNullOrWhiteSpace(pib))
        {
            var lokalniPartner = partneri.FirstOrDefault(p => p.PIB == pib);
            if (lokalniPartner is not null)
            {
                naziv = lokalniPartner.Naziv ?? registrationName ?? "";
            }
            else
            {
                naziv = registrationName ?? "";
                upozorenja.Add(pib);
            }
        }
        else
        {
            naziv = registrationName ?? "";
        }

        red.naziv            = naziv;
        red.PIB               = pib;
        red.tipDokumenta      = PrevediTipRacuna(tipKod);
        red.invoiceDateUtc    = datumSlanja;
        red.paymentDateUtc    = datumValute;
        red.accountingDateUtc = datumPrometa;
        red.Osnovica          = osnovica;
        red.PDV               = pdvIznos;
        red.Ukupno            = vrednost;
        red.brojDokumenta     = brojRacuna;
    }

    // Prvi element sa datim lokalnim imenom bilo gde unutar scope-a (rekurzivno).
    private static string? Vrednost(XElement? scope, string localName)
        => scope?.Descendants().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;

    private static XElement? Podelement(XElement? scope, string localName)
        => scope?.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);

    // Svi elementi sa datim lokalnim imenom bilo gde unutar scope-a (rekurzivno) —
    // za AdditionalDocumentReference može ih biti do 3.
    private static IEnumerable<XElement> Elementi(XElement? scope, string localName)
        => scope?.Descendants().Where(e => e.Name.LocalName == localName) ?? [];

    private static decimal? ParsirajDecimal(string? raw)
        => decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static DateTime? ParsirajDatum(string? raw)
        => DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    // 380/381/383/386 — NOVA terminologija za nove upise (stara KNJIZNO ODOBRENJE/ZADUZENJE
    // ostaje samo u starim podacima, filter u GetListaAsync i dalje prepoznaje oba).
    // "AVANSNA FAKTURA" usklađeno sa formom /efakture/unos i Izlaznim e-fakturama.
    private static string PrevediTipRacuna(string? kod) => kod switch
    {
        "380" => "FAKTURA",
        "381" => "DOKUMENT O SMANJENJU",
        "383" => "DOKUMENT O POVECANJU",
        "386" => "AVANSNA FAKTURA",
        _     => kod ?? ""
    };

    // Ekvivalent Class_eFakturaPrevodi.PurchaseInvoiceStatusPrevod
    private static string PrevediStatus(string? apiStatus) => apiStatus switch
    {
        "New"        => "Novo",
        "Seen"       => "Pregledano",
        "Reminded"   => "Podsetnik poslat",
        "ReNotified" => "Ponovo Obavešten",
        "Received"   => "Primljeno",
        "Deleted"    => "Obrisano",
        "Approved"   => "Odobreno",
        "Rejected"   => "Odbijeno",
        "Cancelled"  => "Otkazano",
        "Storno"     => "Stornirano",
        "Unknown"    => "Nepoznato",
        _            => apiStatus ?? ""
    };

    // ── Osveži status ─────────────────────────────────────────────────────────
    public async Task<string?> OsveziStatusAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        var statusDto = await _api.GetAsync<PurchaseInvoiceStatusDto>(
            apiKey, tipServera, $"purchase-invoice?invoiceId={invoiceId}");

        var red = await _db.EFaktureUlaz.FirstOrDefaultAsync(x => x.invoiceID == invoiceId);
        if (red is null) return null;

        red.statusDokumenta = PrevediStatus(statusDto?.Status);
        await _db.SaveChangesAsync();

        return red.statusDokumenta;
    }

    // ── Prihvati / odbij ──────────────────────────────────────────────────────
    public async Task<(bool uspesno, string poruka)> PrihvatiOdbijAsync(string invoiceId, bool prihvati, string komentar)
    {
        if (!prihvati && string.IsNullOrWhiteSpace(komentar))
            return (false, "Ne možete odbiti dokument bez komentara");

        if (!long.TryParse(invoiceId, out var idLong))
            return (false, "Neispravan ID dokumenta.");

        var (apiKey, tipServera) = await GetSettings();

        var telo = new AcceptRejectRequestDto
        {
            InvoiceId = idLong,
            Accepted  = prihvati,
            Comment   = komentar ?? ""
        };

        var odgovor = await _api.PostStringAsync(
            apiKey, tipServera, "purchase-invoice/acceptRejectPurchaseInvoice", telo);

        // Uspešan odgovor kod ovog endpointa nije uvek JSON; greška JESTE JSON sa ErrorCode.
        try
        {
            var node = JsonNode.Parse(odgovor);
            if (node?["ErrorCode"] is not null)
            {
                var errorCode = node["ErrorCode"]?.GetValue<string>();
                var message   = node["Message"]?.GetValue<string>();
                var poruka    = _greskaPrevod.Prevedi(errorCode, message);
                return (false, poruka);
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Odgovor nije JSON — tretira se kao uspeh.
        }

        await OsveziStatusAsync(invoiceId);
        return (true, "");
    }

    // ── PDF (ugrađen u envelope XML) ──────────────────────────────────────────
    public async Task<byte[]?> PreuzmiPdfAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        var xml = await _api.GetStringAsync(apiKey, tipServera, $"purchase-invoice/xml?invoiceId={invoiceId}");

        var root      = XDocument.Parse(xml).Root;
        var header    = Podelement(root, "DocumentHeader");
        var pdfBase64 = Vrednost(header, "DocumentPdf");

        if (string.IsNullOrWhiteSpace(pdfBase64)) return null;

        try
        {
            return Convert.FromBase64String(pdfBase64);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    // ── XML (ceo envelope, bez izdvajanja) ────────────────────────────────────
    public async Task<string?> PreuzmiXmlAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        return await _api.GetStringAsync(apiKey, tipServera, $"purchase-invoice/xml?invoiceId={invoiceId}");
    }

    // ── Prateći dokumenti (prilozi) — cac:AdditionalDocumentReference, do 3 po fakturi ──
    public async Task<List<PrateciDokument>> UcitajPrateceDokumenteAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        var xml  = await _api.GetStringAsync(apiKey, tipServera, $"purchase-invoice/xml?invoiceId={invoiceId}");
        var root = XDocument.Parse(xml).Root;

        var rezultat = new List<PrateciDokument>();

        foreach (var docRef in Elementi(root, "AdditionalDocumentReference"))
        {
            // Neke reference nemaju ugrađen sadržaj (npr. samo broj narudžbenice) —
            // uzimamo samo one koje stvarno nose Base64 PDF prilog.
            var attachment = Podelement(docRef, "Attachment");
            var base64     = Vrednost(attachment, "EmbeddedDocumentBinaryObject");
            if (string.IsNullOrWhiteSpace(base64)) continue;

            rezultat.Add(new PrateciDokument
            {
                Naziv         = Vrednost(docRef, "ID") ?? "Prilog",
                Base64Sadrzaj = base64
            });
        }

        return rezultat;
    }
}
