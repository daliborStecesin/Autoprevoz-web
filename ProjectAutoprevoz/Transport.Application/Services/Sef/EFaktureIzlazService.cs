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
/// Izlazne e-fakture — lokalna lista (tbl_eInvoice) + SEF sinhronizacija
/// (sales-invoice/ids, /xml, status). Storno/Otkaži/PDF/XML export i integracija
/// sa karticom dolaze u narednim fazama.
/// </summary>
public class EFaktureIzlazService : IEFaktureIzlazService
{
    private readonly TransportDbContext _db;
    private readonly SefApiClient       _api;
    private readonly IPartnerService    _partnerService;

    public EFaktureIzlazService(TransportDbContext db, SefApiClient api, IPartnerService partnerService)
    {
        _db             = db;
        _api            = api;
        _partnerService = partnerService;
    }

    private async Task<(string apiKey, string tipServera)> GetSettings()
    {
        var pod = await _db.Podesavanja.AsNoTracking().FirstOrDefaultAsync();
        return (pod?.sefApiKey ?? string.Empty, pod?.sefTipServera ?? "DEMO");
    }

    public async Task<List<EInvoice>> GetListaAsync(
        string? nazivIliPibFilter,
        string? brojDokumentaFilter,
        string? tipDokumentaFilter,
        string? statusFilter,
        string tipDatuma,
        DateTime? datumOd,
        DateTime? datumDo)
    {
        var q = _db.EInvoices.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nazivIliPibFilter))
            q = q.Where(x => (x.partner != null && x.partner.Contains(nazivIliPibFilter))
                           || (x.pib     != null && x.pib.Contains(nazivIliPibFilter)));

        if (!string.IsNullOrWhiteSpace(brojDokumentaFilter))
            q = q.Where(x => x.brojDokumenta != null && x.brojDokumenta.Contains(brojDokumentaFilter));

        if (!string.IsNullOrWhiteSpace(tipDokumentaFilter) && tipDokumentaFilter != "SVI TIPOVI")
        {
            // Ista tolerancija stare/nove terminologije kao kod Ulaznih e-faktura —
            // izlazne takođe imaju stare upise sa KNJIZNO ODOBRENJE/ZADUZENJE u bazi.
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

        q = poPrometu
            ? q.OrderByDescending(x => x.accountingDateUtc)
            : q.OrderByDescending(x => x.invoiceDateUtc);

        return await q.ToListAsync();
    }

    // ── Sinhronizacija (1:1 prevod button1_Click iz frm_eFakturaLista) ──────
    public async Task<(int brojNovih, List<string> upozorenjaPartner)> SinhronizujAsync(DateTime datumOd, DateTime datumDo)
    {
        var (apiKey, tipServera) = await GetSettings();
        var upozorenja = new List<string>();

        var endpointIds = $"sales-invoice/ids?dateFrom={datumOd:yyyy-MM-dd}&dateTo={datumDo:yyyy-MM-dd}";
        // Isto kao stari SalesInvoiceIDs — POST sa praznim telom (isto kao PurchaseInvoiceIDs),
        // iako parametri opsega datuma idu kroz query string. GET vraća 405.
        var idsDto = await _api.PostEmptyAsync<SalesInvoiceIdsDto>(apiKey, tipServera, endpointIds);

        if (idsDto?.SalesInvoiceIds is null or { Count: 0 })
            return (0, upozorenja);

        // Dedupe u memoriji (ne preko .Contains() nad parametrizovanom listom u EF upitu) —
        // EF Core 8 prevodi takav .Contains() u OPENJSON(...) WITH (...), što ovaj SQL Server
        // (niži compatibility level) ne podržava ("Incorrect syntax near 'WITH'").
        var postojeciIds = (await _db.EInvoices
                .Where(x => x.invoiceID != null)
                .Select(x => x.invoiceID!)
                .ToListAsync())
            .ToHashSet();

        var sviPartneri = await _partnerService.GetSviPartneriAsync();

        var upisano = 0;

        foreach (var id in idsDto.SalesInvoiceIds)
        {
            var invoiceIdStr = id.ToString();
            if (postojeciIds.Contains(invoiceIdStr)) continue;

            var red = new EInvoice
            {
                idRacuna       = null,
                invoiceID      = invoiceIdStr,
                salesInvoiceID = invoiceIdStr,
                invoiceIDint   = id
            };

            try
            {
                var xml = await _api.GetStringAsync(apiKey, tipServera, $"sales-invoice/xml?invoiceId={id}");
                PopuniIzXml(red, xml, sviPartneri, upozorenja);

                var statusDto = await _api.GetAsync<SalesInvoiceStatusDto>(
                    apiKey, tipServera, $"sales-invoice?invoiceId={id}");
                red.statusDokumenta = PrevediStatus(statusDto?.Status);
            }
            catch
            {
                // Isto kao stari kod — pojedinačan XML/parsiranje koji padne ne prekida ceo
                // proces, red se upisuje sa onim što je uspelo do greške + fallback status.
                red.statusDokumenta ??= "Nepoznato";
            }

            _db.EInvoices.Add(red);
            postojeciIds.Add(invoiceIdStr);
            upisano++;
        }

        await _db.SaveChangesAsync();
        return (upisano, upozorenja);
    }

    private static void PopuniIzXml(EInvoice red, string xml, List<Partner> partneri, List<string> upozorenja)
    {
        var root = XDocument.Parse(xml).Root;

        // Ceo XML je zapravo ENVELOPE — stvarni Invoice/CreditNote sadržaj je UGNJEŽDEN,
        // ne na root nivou. Zato tražimo rekurzivno kroz ceo dokument (isto kao stari
        // GetElementsByTagName — uvek globalna/rekurzivna pretraga, uzima prvi nađen
        // element, bez obzira na dubinu). Isti pattern kao Ulazne e-fakture.
        var brojRacuna = Vrednost(root, "ID");

        var issueDateStr = Vrednost(root, "IssueDate");
        var dueDateStr   = Vrednost(root, "DueDate");

        var tipKod = Vrednost(root, "InvoiceTypeCode") ?? Vrednost(root, "CreditNoteTypeCode");

        // KLJUČNA RAZLIKA od Ulaznih: kupac je AccountingCustomerParty (ne Supplier),
        // pošto smo mi prodavac koji šalje fakturu kupcu.
        var customerParty = Podelement(root, "AccountingCustomerParty");
        var pib              = Vrednost(customerParty, "EndpointID");
        var registrationName = Vrednost(customerParty, "RegistrationName");

        // Datum prometa: cac:Delivery > cbc:ActualDeliveryDate (ugnježdeno u Delivery,
        // ne direktno na root nivou kao kod Ulaznih).
        var delivery         = Podelement(root, "Delivery");
        var deliveryDateStr  = Vrednost(delivery, "ActualDeliveryDate");

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

        red.partner           = naziv;
        red.pib               = pib;
        red.tipDokumenta      = PrevediTipRacuna(tipKod);
        red.invoiceDateUtc    = datumSlanja;
        red.paymentDateUtc    = datumValute;
        red.accountingDateUtc = datumPrometa;
        red.sumWithoutVat     = osnovica;
        red.vatSum            = pdvIznos;
        red.sumWithVat        = vrednost;
        red.brojDokumenta     = brojRacuna;
    }

    // Prvi element sa datim lokalnim imenom bilo gde unutar scope-a (rekurzivno).
    private static string? Vrednost(XElement? scope, string localName)
        => scope?.Descendants().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;

    private static XElement? Podelement(XElement? scope, string localName)
        => scope?.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);

    private static decimal? ParsirajDecimal(string? raw)
        => decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static DateTime? ParsirajDatum(string? raw)
        => DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    // 380/381/383/386 — ista mapa kao Ulazne e-fakture (nova terminologija za nove upise).
    private static string PrevediTipRacuna(string? kod) => kod switch
    {
        "380" => "FAKTURA",
        "381" => "DOKUMENT O SMANJENJU",
        "383" => "DOKUMENT O POVECANJU",
        "386" => "AVANS",
        _     => kod ?? ""
    };

    // Ekvivalent Class_eFakturaPrevodi.statusPrevod (NE PurchaseInvoiceStatusPrevod —
    // različit set vrednosti za izlazne e-fakture).
    private static string PrevediStatus(string? apiStatus) => apiStatus switch
    {
        "New"      => "Novi",
        "Draft"    => "Priprema",
        "Sent"     => "Poslato",
        "Paid"     => "Placeno",
        "Mistake"  => "Greska",
        "OverDue"  => "Van Valute",
        "Archived" => "Arhivirano",
        "Sending"  => "Slanje",
        "Deleted"  => "Obrisano",
        "Approved" => "Prihvaceno",
        "Rejected" => "Odbijeno",
        "Cancelled"=> "Otkazano",
        "Storno"   => "Stornirano",
        "Unknown"  => "Nepoznato",
        _          => apiStatus ?? ""
    };

    // ── Osveži status ─────────────────────────────────────────────────────────
    public async Task<string?> OsveziStatusAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        var statusDto = await _api.GetAsync<SalesInvoiceStatusDto>(
            apiKey, tipServera, $"sales-invoice?invoiceId={invoiceId}");

        var red = await _db.EInvoices.FirstOrDefaultAsync(x => x.invoiceID == invoiceId);
        if (red is null) return null;

        red.statusDokumenta = PrevediStatus(statusDto?.Status);
        await _db.SaveChangesAsync();

        return red.statusDokumenta;
    }

    // ── Komentar odbijanja (live, za Fazu C) ─────────────────────────────────
    public async Task<string?> UzmiKomentarOdbijanjaAsync(string invoiceId)
    {
        var (apiKey, tipServera) = await GetSettings();
        var statusDto = await _api.GetAsync<SalesInvoiceStatusDto>(
            apiKey, tipServera, $"sales-invoice?invoiceId={invoiceId}");

        return statusDto?.Comment;
    }

    // ── Storno ────────────────────────────────────────────────────────────────
    public async Task<(bool uspesno, string poruka)> StornoAsync(string invoiceId, string komentar)
    {
        if (string.IsNullOrWhiteSpace(komentar))
            return (false, "Da biste stornirali E-Fakturu, morate upisati komentar i zatim kliknuti na POTVRDI ZAHTEV");

        if (!long.TryParse(invoiceId, out var idLong))
            return (false, "Neispravan ID dokumenta.");

        var (apiKey, tipServera) = await GetSettings();

        var telo = new StornoRequestDto { InvoiceId = idLong, StornoComment = komentar };
        var odgovor = await _api.PostStringAsync(apiKey, tipServera, "sales-invoice/storno", telo);

        // Uspešan odgovor sadrži "Status"; greška sadrži "ErrorCode" — ove hardkodovane
        // poruke su specifične za Storno i NE prolaze kroz IGreskaEfakturaPrevodService.
        try
        {
            var node = JsonNode.Parse(odgovor);
            if (node?["ErrorCode"] is not null)
            {
                var errorCode = node["ErrorCode"]?.GetValue<string>();
                var poruka = errorCode switch
                {
                    "InvoiceNotInAppropriateStatusToBeReversed" => "Dokument sa ovim statusom se ne može Stornirati!",
                    _ => node["Message"]?.GetValue<string>() ?? odgovor
                };
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

    // ── Otkaži ────────────────────────────────────────────────────────────────
    public async Task<(bool uspesno, string poruka)> OtkaziAsync(string invoiceId, string komentar)
    {
        if (string.IsNullOrWhiteSpace(komentar))
            return (false, "Da biste otkazali slanje E-Fakture, morate upisati komentar i zatim kliknuti na POTVRDI ZAHTEV");

        if (!long.TryParse(invoiceId, out var idLong))
            return (false, "Neispravan ID dokumenta.");

        var (apiKey, tipServera) = await GetSettings();

        var telo = new CancelRequestDto { InvoiceId = idLong, CancelComments = komentar };
        var odgovor = await _api.PostStringAsync(apiKey, tipServera, "sales-invoice/cancel", telo);

        try
        {
            var node = JsonNode.Parse(odgovor);
            if (node?["ErrorCode"] is not null)
            {
                var errorCode = node["ErrorCode"]?.GetValue<string>();
                var poruka = errorCode switch
                {
                    "InvoiceForCancellationNotInSpecificStatus" =>
                        "Dokument sa ovim statusom se ne može otkazati!\nKorisnik može otkazati dokument u statusu Novi,Priprema,Greska,\nili ako je u statusu Slanje duže od 15 minuta!",
                    "InvoiceForCancellationNotFound" =>
                        "Dokument koji ste odabrali nije pronađen",
                    "InvoiceForCancellationNotLongEnoughInSendingStatus" =>
                        "Dokument koji ste odabrali nije dovoljno dugo u statusu SLANJE da bi bio otkazan\nDa bi se otkazao. mora biti najmanje 15 minuta u statusu SLANJE",
                    _ => node["Message"]?.GetValue<string>() ?? odgovor
                };
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
        var xml = await _api.GetStringAsync(apiKey, tipServera, $"sales-invoice/xml?invoiceId={invoiceId}");

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
        return await _api.GetStringAsync(apiKey, tipServera, $"sales-invoice/xml?invoiceId={invoiceId}");
    }
}
