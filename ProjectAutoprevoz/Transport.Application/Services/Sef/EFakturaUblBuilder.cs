using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Transport.Application.Services.Sef.Models;

namespace Transport.Application.Services.Sef;

// Gradi UBL XML za FAKTURU i AVANSNU FAKTURU, redosled elemenata prati desktop
// Class_E_Racun (već prihvaćen na SEF-u). Podržava standardne PDV stavke
// (S20/S10) i oslobođene stavke (0%, grupisano po slovu kategorije —
// Z/O/OE/E/AE/SS/R/N itd.). Bez knjižnih — dodaju se u kasnijim koracima.
// Prateća dokumenta (PDF, max 3, Base64 već gotov u memoriji) SU podržana.
// Čist string builder, bez baze i bez slanja. XElement escapuje &,<,>.
public class EFakturaUblBuilder : IEFakturaUblBuilder
{
    private static readonly XNamespace Ns  = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    // DOKUMENT O SMANJENJU koristi <CreditNote> kao root, sa sopstvenim default
    // namespace-om — cec/cac/cbc/xsi/xsd/sbt ostaju identični (odvojeni, prefiksirani).
    private static readonly XNamespace NsCreditNote = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace Cec = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private static readonly XNamespace Xsd = "http://www.w3.org/2001/XMLSchema";
    private static readonly XNamespace Sbt = "http://mfin.gov.rs/srbdt/srbdtext";

    public string Build(EFakturaUblInput input)
    {
        var jeAvans      = input.TipDokumenta == "AVANSNA FAKTURA";
        var jePovecanje  = input.TipDokumenta == "DOKUMENT O POVECANJU";
        var jeSmanjenje  = input.TipDokumenta == "DOKUMENT O SMANJENJU";

        var osnovicaUkupno = input.Stavke.Sum(s => s.Osnovica);
        var pdvUkupno       = input.Stavke.Sum(s => s.Pdv);
        var ukupnoSaPdv     = osnovicaUkupno + pdvUkupno;

        // Avansi sa bar jednom iskorišćenom kategorijom (osnovica uneta > 0) —
        // izabrani avans bez ijednog unetog iznosa se u potpunosti ignoriše
        // (kao da nije ni izabran).
        var iskorisceniAvansi = input.IzabraniAvansi
            .Select(a => new FiltriranAvans(a.BrojDokumenta, a.DatumIzdavanja,
                [.. a.Kategorije.Where(k => k.IskorisenaOsnovica > 0)]))
            .Where(a => a.Kategorije.Count > 0)
            .ToList();

        var imaAvansniOdbitak = iskorisceniAvansi.Count > 0;
        var prepaidAmount = iskorisceniAvansi.Sum(a => a.Kategorije.Sum(k => k.IskorisenaOsnovica + k.IskorisenPdv));

        var rootNs  = jeSmanjenje ? NsCreditNote : Ns;
        var rootIme = jeSmanjenje ? "CreditNote" : "Invoice";

        var invoice = new XElement(rootNs + rootIme,
            new XAttribute(XNamespace.Xmlns + "cec", Cec.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "cac", Cac.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "cbc", Cbc.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "xsd", Xsd.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "sbt", Sbt.NamespaceName),

            // Avansni odbitak (UBLExtensions/SrbDtExt) MORA biti prvi element u
            // <Invoice>, pre CustomizationID — SEF ekstenzija, van standardnog UBL reda.
            imaAvansniOdbitak ? SrbDtExt(iskorisceniAvansi, input, osnovicaUkupno, pdvUkupno, ukupnoSaPdv, prepaidAmount) : null,

            new XElement(Cbc + "CustomizationID", "urn:cen.eu:en16931:2017#compliant#urn:mfin.gov.rs:srbdt:2022"),
            new XElement(Cbc + "ID", Cln(input.BrojDokumenta)),
            new XElement(Cbc + "IssueDate", DateTime.Today.ToString("yyyy-MM-dd")),
            // Smanjenje (CreditNote) nikad nema DueDate.
            jeSmanjenje ? null : Tag(Cbc + "DueDate", input.DatumValute?.ToString("yyyy-MM-dd")),
            jeSmanjenje
                ? new XElement(Cbc + "CreditNoteTypeCode", "381")
                : new XElement(Cbc + "InvoiceTypeCode", jeAvans ? "386" : jePovecanje ? "383" : "380"),
            NoteTag(input),
            new XElement(Cbc + "DocumentCurrencyCode", "RSD"),
            Tag(Cbc + "BuyerReference", input.InterniBrojZaRutiranje),

            jeAvans ? InvoicePeriodZaAvans(input)
                : jePovecanje ? InvoicePeriodZaPovecanje(input)
                : jeSmanjenje ? InvoicePeriodZaSmanjenje(input)
                : InvoicePeriod(input.NastanakPdvObaveze),
            RefTag(Cac + "OrderReference", input.BrojNarudzbenice),
            // BillingReference (avans/povećanje/smanjenje) ide POSLE OrderReference,
            // PRE Originator/Contract — strogi UBL redosled elemenata (SEF šema).
            imaAvansniOdbitak ? iskorisceniAvansi.Select(BillingReference) : null,
            jePovecanje && input.PovecanjeOdnosiSeNa == "Pojedinačna faktura"
                ? input.PovecanjeIzvorneFakture.Select(BillingReferenceIzvornaFaktura)
                : null,
            jeSmanjenje && input.SmanjenjeOdnosiSeNa is "Pojedinačna faktura" or "Pojedinačna avansna faktura"
                ? input.SmanjenjeIzvorneFakture.Select(BillingReferenceIzvornaFaktura)
                : null,
            RefTag(Cac + "OriginatorDocumentReference", input.BrojTendera),
            RefTag(Cac + "ContractDocumentReference", input.BrojUgovora),

            input.Prilozi.Select(AdditionalDocumentReference),

            AccountingSupplierParty(input),
            AccountingCustomerParty(input),

            // Avans nema datum prometa — Delivery se nikad ne emituje za taj tip.
            // Povećanje ima sopstveno pravilo (DeliveryZaPovecanje). Smanjenje UVEK
            // emituje Delivery = Datum smanjenja, za sva 3 moda, bez grananja.
            jeAvans ? null
                : jePovecanje ? DeliveryZaPovecanje(input)
                : jeSmanjenje ? Delivery(input.SmanjenjeDatumSmanjenja)
                : Delivery(input.DatumPrometa),
            PaymentMeans(input),
            TaxTotal(input, pdvUkupno),
            LegalMonetaryTotal(osnovicaUkupno, ukupnoSaPdv, prepaidAmount),

            input.Stavke.Select((s, i) => InvoiceLine(s, i + 1, jeAvans, jeSmanjenje))
        );

        var sb = new StringBuilder();
        // OmitXmlDeclaration=true + ručni prefiks: XmlWriter nad StringBuilder-om
        // uvek prijavljuje encoding="utf-16" (TextWriter je interno UTF-16) bez
        // obzira na XmlWriterSettings.Encoding — a fajl se stvarno šalje kao UTF-8
        // (Blob u downloadTextFile), pa deklaracija mora ručno da kaže UTF-8.
        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings
               {
                   Indent = true,
                   OmitXmlDeclaration = true
               }))
        {
            new XDocument(invoice).Save(writer);
        }

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine + sb;
    }

    // Trim primenjen na SVE tekstualne vrednosti pre upisa u XML (vodeći/prateći
    // razmaci nikad ne smeju da se pojave u izvezenom dokumentu).
    private static string Cln(string? s) => (s ?? "").Trim();

    // Za adrese — dodatno sažima uzastopne razmake unutar teksta u jedan
    // (npr. "JAKOVA IGNJATOVICA   230" -> "JAKOVA IGNJATOVICA 230").
    private static string ClnAdresa(string? s) => Regex.Replace(Cln(s), @"\s{2,}", " ");

    private static XElement? Tag(XName tag, string? value)
    {
        var v = Cln(value);
        return v.Length == 0 ? null : new XElement(tag, v);
    }

    private static XElement? RefTag(XName tag, string? id)
    {
        var v = Cln(id);
        return v.Length == 0 ? null : new XElement(tag, new XElement(Cbc + "ID", v));
    }

    private static XElement? StreetTag(string? adresa)
    {
        var v = ClnAdresa(adresa);
        return v.Length == 0 ? null : new XElement(Cbc + "StreetName", v);
    }

    private static XElement Amount(XName tag, decimal value) =>
        new(tag, new XAttribute("currencyID", "RSD"), F2(value));

    private static string F2(decimal v) => v.ToString("F2", CultureInfo.InvariantCulture);
    private static string F4(decimal v) => v.ToString("F4", CultureInfo.InvariantCulture);

    // Note se gradi uslovno — svaka labela ("Vozilo:"/"CMR:") se dodaje SAMO ako ta
    // vrednost postoji, da se ne pojavi prazna labela kad je polje prazno. Ceo tag
    // se izostavlja ako posle svega ostane prazan string.
    private static XElement? NoteTag(EFakturaUblInput input)
    {
        var sb = new StringBuilder();

        var vozilo = Cln(input.Vozilo);
        if (vozilo.Length > 0)
            sb.Append("Vozilo: ").Append(vozilo).Append(' ');

        var cmr = Cln(input.BrojCmr);
        if (cmr.Length > 0)
            sb.Append("CMR: ").Append(cmr).Append(' ');

        var komentar = Cln(input.Komentar);
        if (komentar.Length > 0)
            sb.Append(komentar);

        var tekst = sb.ToString().Trim();
        return tekst.Length == 0 ? null : new XElement(Cbc + "Note", tekst);
    }

    private static XElement? InvoicePeriod(string nastanakPdvObaveze)
    {
        var code = nastanakPdvObaveze switch
        {
            "Datum izdavanja" => "3",
            "Datum prometa"   => "35",
            "Datum plaćanja"  => "432",
            _                 => null // "Ne nastaje PDV obaveza" -> izostavi ceo InvoicePeriod
        };

        return code is null ? null : new XElement(Cac + "InvoicePeriod", new XElement(Cbc + "DescriptionCode", code));
    }

    // Avans NE koristi mapiranje 3/35/432 od "Nastanak PDV obaveze" (to polje je
    // na formi fiksirano na "Datum plaćanja" bez obzira na stavke) — kod je uvek
    // fiksno 432 kad ima bar jedna S-stavka (PDV), i izostavljeno kad su SVE
    // stavke 0% (isto pravilo omisije kao kod FAKTURE).
    private static XElement? InvoicePeriodZaAvans(EFakturaUblInput input)
    {
        var imaPdv = input.Stavke.Any(s => KategorijaZaStavku(s).Id == "S");
        return imaPdv ? new XElement(Cac + "InvoicePeriod", new XElement(Cbc + "DescriptionCode", "432")) : null;
    }

    // Dokument o povećanju: InvoicePeriod NE koristi generičko "Nastanak PDV obaveze"
    // mapiranje (3/35/432 od FAKTURE) — ima sopstveni izbor sa forme: mod
    // (pojedinačna/period) + vrsta datuma (ugovor=35 / zaračunavanje troškova=3).
    // Potvrđeno na 6 realnih primera: mod "Fakture u periodu" UVEK emituje
    // Start/EndDate (bez obzira na PDV stanje) — DescriptionCode se dodaje SAMO
    // kad PDV se obračunava. Mod "Pojedinačna faktura" nema Start/End uopšte —
    // ili samo DescriptionCode (PDV se obračunava) ili ništa ("Ne nastaje").
    private static XElement? InvoicePeriodZaPovecanje(EFakturaUblInput input)
    {
        var imaPdv = input.PovecanjeNastanakPdv == "PDV se obračunava";
        var code   = input.PovecanjeVrstaDatuma == "Datum povećanja - ugovor" ? "35" : "3";

        if (input.PovecanjeOdnosiSeNa == "Fakture u periodu")
        {
            var sadrzaj = new List<XElement?>
            {
                Tag(Cbc + "StartDate", input.PovecanjePeriodOd?.ToString("yyyy-MM-dd")),
                Tag(Cbc + "EndDate", input.PovecanjePeriodDo?.ToString("yyyy-MM-dd"))
            };

            if (imaPdv)
                sadrzaj.Add(new XElement(Cbc + "DescriptionCode", code));

            return new XElement(Cac + "InvoicePeriod", sadrzaj);
        }

        // Pojedinačna faktura — nema Start/End; DescriptionCode samo kad PDV se obračunava.
        return imaPdv ? new XElement(Cac + "InvoicePeriod", new XElement(Cbc + "DescriptionCode", code)) : null;
    }

    // Dokument o smanjenju: "Pojedinačna faktura"/"Pojedinačna avansna faktura" NIKAD
    // nemaju InvoicePeriod; "Fakture u periodu" UVEK ga emituje sa Start/EndDate,
    // BEZ DescriptionCode (kod smanjenja taj kod ne postoji nikad).
    private static XElement? InvoicePeriodZaSmanjenje(EFakturaUblInput input) =>
        input.SmanjenjeOdnosiSeNa == "Fakture u periodu"
            ? new XElement(Cac + "InvoicePeriod",
                Tag(Cbc + "StartDate", input.SmanjenjePeriodOd?.ToString("yyyy-MM-dd")),
                Tag(Cbc + "EndDate", input.SmanjenjePeriodDo?.ToString("yyyy-MM-dd")))
            : null;

    // Delivery/ActualDeliveryDate — kontroliše ISKLJUČIVO pod-padajući "Vrsta datuma",
    // nezavisno od (auto-izvedenog) glavnog PDV stanja: "Datum povećanja - ugovor" ->
    // UVEK Delivery, čak i kad je PDV stanje "Ne nastaje"; "zaračunavanje troškova" ->
    // NIKAD Delivery. Potvrđeno na 6 realnih primera.
    private static XElement? DeliveryZaPovecanje(EFakturaUblInput input) =>
        input.PovecanjeVrstaDatuma == "Datum povećanja - ugovor" ? Delivery(input.PovecanjeDatumUgovor) : null;

    // Izvorna faktura iz naše baze ima IssueDate; ručno uneti broj (fakture koje
    // nisu registrovane na E-fakturi) nema datum — Tag() ga izostavlja kad je null.
    private static XElement BillingReferenceIzvornaFaktura(EFakturaUblIzvornaFaktura f) =>
        new(Cac + "BillingReference",
            new XElement(Cac + "InvoiceDocumentReference",
                new XElement(Cbc + "ID", Cln(f.BrojDokumenta)),
                Tag(Cbc + "IssueDate", f.DatumIzdavanja?.ToString("yyyy-MM-dd"))));

    // Prilog je već Base64 u memoriji (konvertovano pri dodavanju, ne ovde) — builder
    // ga samo ugrađuje. ID = ime fajla, isto kao Class_E_Racun.
    private static XElement AdditionalDocumentReference(EFakturaUblPrilog prilog) =>
        new(Cac + "AdditionalDocumentReference",
            new XElement(Cbc + "ID", Cln(prilog.ImeFajla)),
            new XElement(Cac + "Attachment",
                new XElement(Cbc + "EmbeddedDocumentBinaryObject",
                    new XAttribute("mimeCode", "application/pdf"),
                    new XAttribute("filename", Cln(prilog.ImeFajla)),
                    prilog.Base64)));

    private static XElement AccountingSupplierParty(EFakturaUblInput input)
    {
        var pib = Cln(input.ProdavacPib);

        return new XElement(Cac + "AccountingSupplierParty",
            new XElement(Cac + "Party",
                new XElement(Cbc + "EndpointID", new XAttribute("schemeID", "9948"), pib),
                new XElement(Cac + "PartyName", new XElement(Cbc + "Name", Cln(input.ProdavacNaziv))),
                new XElement(Cac + "PostalAddress",
                    StreetTag(input.ProdavacAdresa),
                    Tag(Cbc + "CityName", input.ProdavacMesto),
                    Tag(Cbc + "PostalZone", input.ProdavacPostanskiBroj),
                    new XElement(Cac + "Country", new XElement(Cbc + "IdentificationCode", "RS"))),
                new XElement(Cac + "PartyTaxScheme",
                    new XElement(Cbc + "CompanyID", "RS" + pib),
                    new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT"))),
                new XElement(Cac + "PartyLegalEntity",
                    new XElement(Cbc + "RegistrationName", Cln(input.ProdavacNaziv)),
                    new XElement(Cbc + "CompanyID", PadMaticni(input.ProdavacMaticniBroj)))));
    }

    // Kupac: JBKJS (ako postoji) ide ODMAH posle EndpointID. Adresa/mesto/pošt.
    // se emituju samo ako partner ima tu vrednost (poštanski broj partner
    // entitet trenutno uopšte nema, pa se taj tag nikad ne pojavljuje).
    private static XElement AccountingCustomerParty(EFakturaUblInput input)
    {
        var pib   = Cln(input.KupacPib);
        var jbkjs = Cln(input.KupacJbkjs);

        var sadrzaj = new List<XElement?>
        {
            new(Cbc + "EndpointID", new XAttribute("schemeID", "9948"), pib),
            jbkjs.Length == 0
                ? null
                : new XElement(Cac + "PartyIdentification", new XElement(Cbc + "ID", $"JBKJS:{jbkjs}")),
            new(Cac + "PartyName", new XElement(Cbc + "Name", Cln(input.KupacNaziv))),
            new XElement(Cac + "PostalAddress",
                StreetTag(input.KupacAdresa),
                Tag(Cbc + "CityName", input.KupacMesto),
                Tag(Cbc + "PostalZone", input.KupacPostanskiBroj),
                new XElement(Cac + "Country", new XElement(Cbc + "IdentificationCode", "RS"))),
            new(Cac + "PartyTaxScheme",
                new XElement(Cbc + "CompanyID", "RS" + pib),
                new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT"))),
            new(Cac + "PartyLegalEntity",
                new XElement(Cbc + "RegistrationName", Cln(input.KupacNaziv)),
                new XElement(Cbc + "CompanyID", PadMaticni(input.KupacMaticniBroj)))
        };

        return new XElement(Cac + "AccountingCustomerParty", new XElement(Cac + "Party", sadrzaj));
    }

    private static XElement? Delivery(DateTime? datumPrometa) =>
        datumPrometa is null
            ? null
            : new XElement(Cac + "Delivery", new XElement(Cbc + "ActualDeliveryDate", datumPrometa.Value.ToString("yyyy-MM-dd")));

    private static XElement PaymentMeans(EFakturaUblInput input) =>
        new(Cac + "PaymentMeans",
            new XElement(Cbc + "PaymentMeansCode", "30"),
            Tag(Cbc + "PaymentID", input.PozivNaBroj),
            new XElement(Cac + "PayeeFinancialAccount", new XElement(Cbc + "ID", Cln(input.ZiroRacunIzdavaoca))));

    // Kategorija PDV svrhe grupisanja/TaxCategory — IZVODI se iz PdvKategorija
    // (model je ne preračunava builder): "S20"/"S10" -> ID="S" + procenat; svako
    // drugo slovo (oslobođenje) -> ID=to slovo + procenat 0.
    private static (string Id, decimal Procenat) KategorijaZaStavku(EFakturaUblStavka s) => s.PdvKategorija switch
    {
        "S20" => ("S", 20m),
        "S10" => ("S", 10m),
        var slovo => (slovo, 0m)
    };

    // Grupisanje PO SLOVU/KATEGORIJI (ne po stavci) — GroupBy čuva redosled PRVOG
    // pojavljivanja ključa kroz stavke (namerno bez sortiranja), isto kao referentni
    // XML-ovi. Oslobođene grupe (ID != "S") dobijaju TaxExemptionReasonCode.
    private static XElement TaxTotal(EFakturaUblInput input, decimal pdvUkupno)
    {
        var subtotali = GrupisiGlavneKategorije(input)
            .Select(k => new XElement(Cac + "TaxSubtotal",
                Amount(Cbc + "TaxableAmount", k.Osnovica),
                Amount(Cbc + "TaxAmount", k.Pdv),
                TaxCategory(input, k.Id, k.Procenat)));

        return new XElement(Cac + "TaxTotal", Amount(Cbc + "TaxAmount", pdvUkupno), subtotali);
    }

    // Iste grupe/redosled kao TaxTotal — izdvojeno da ga ReducedTotals (avans)
    // može ponovo iskoristiti bez duplog GroupBy-a sa drugačijim redosledom.
    private static List<(string Id, decimal Procenat, decimal Osnovica, decimal Pdv)> GrupisiGlavneKategorije(EFakturaUblInput input) =>
        [.. input.Stavke
            .GroupBy(KategorijaZaStavku)
            .Select(g => (g.Key.Id, g.Key.Procenat, Osnovica: g.Sum(s => s.Osnovica), Pdv: g.Sum(s => s.Pdv)))];

    private static XElement TaxCategory(EFakturaUblInput input, string kategorijaId, decimal procenat)
    {
        var elementi = new List<XElement?>
        {
            new(Cbc + "ID", kategorijaId),
            new(Cbc + "Percent", F2(procenat))
        };

        if (kategorijaId != "S")
        {
            var keyClan = input.Oslobodjenja.FirstOrDefault(o => o.Slovo == kategorijaId)?.KeyClan;
            elementi.Add(Tag(Cbc + "TaxExemptionReasonCode", keyClan));
        }

        elementi.Add(new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT")));

        return new XElement(Cac + "TaxCategory", elementi);
    }

    // PrepaidAmount = bruto iskorišćen avansni odbitak (0 kad nema avansa —
    // reprodukuje tačno stari izlaz). PayableAmount = TaxInclusiveAmount - PrepaidAmount.
    private static XElement LegalMonetaryTotal(decimal osnovicaUkupno, decimal ukupnoSaPdv, decimal prepaidAmount) =>
        new(Cac + "LegalMonetaryTotal",
            Amount(Cbc + "LineExtensionAmount", osnovicaUkupno),
            Amount(Cbc + "TaxExclusiveAmount", osnovicaUkupno),
            Amount(Cbc + "TaxInclusiveAmount", ukupnoSaPdv),
            Amount(Cbc + "AllowanceTotalAmount", 0m),
            Amount(Cbc + "PrepaidAmount", prepaidAmount),
            Amount(Cbc + "PayableRoundingAmount", 0m),
            Amount(Cbc + "PayableAmount", ukupnoSaPdv - prepaidAmount));

    // Slovo iz panela Avans-1 ("S20"/"S10"/slovo oslobođenja) -> TaxCategory ID,
    // ista konvencija kao KategorijaZaStavku (S-stope dele ID="S", oslobođenja=slovo).
    private static string AvansKategorijaId(string slovo) => slovo is "S20" or "S10" ? "S" : slovo;

    private sealed record FiltriranAvans(string BrojDokumenta, DateTime? DatumIzdavanja, List<EFakturaUblAvansKategorija> Kategorije);

    // sbt:SrbDtExt — SEF ekstenzija za avansni odbitak: po jedan InvoicedPrepaymentAmount
    // za svaki iskorišćeni avans, zatim JEDNOM zbirni ReducedTotals preko svih avansa.
    private static XElement SrbDtExt(List<FiltriranAvans> avansi, EFakturaUblInput input,
        decimal osnovicaUkupno, decimal pdvUkupno, decimal ukupnoSaPdv, decimal prepaidAmount) =>
        new(Cec + "UBLExtensions",
            new XElement(Cec + "UBLExtension",
                new XElement(Cec + "ExtensionContent",
                    new XElement(Sbt + "SrbDtExt",
                        avansi.Select(InvoicedPrepaymentAmount),
                        ReducedTotals(avansi, input, osnovicaUkupno, pdvUkupno, ukupnoSaPdv, prepaidAmount)))));

    private static XElement InvoicedPrepaymentAmount(FiltriranAvans avans)
    {
        var pdvZaAvans = avans.Kategorije.Sum(k => k.IskorisenPdv);

        // NIKAD TaxExemptionReasonCode ovde — čak ni za oslobođene kategorije
        // (potvrđeno uzorkom); ReasonCode ide samo u glavni/Reduced TaxTotal.
        var subtotali = avans.Kategorije.Select(k => new XElement(Cac + "TaxSubtotal",
            Amount(Cbc + "TaxableAmount", k.IskorisenaOsnovica),
            Amount(Cbc + "TaxAmount", k.IskorisenPdv),
            new XElement(Cac + "TaxCategory",
                new XElement(Cbc + "ID", AvansKategorijaId(k.Slovo)),
                new XElement(Cbc + "Percent", F2(k.Stopa)),
                new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT")))));

        return new XElement(Sbt + "InvoicedPrepaymentAmount",
            new XElement(Cbc + "ID", Cln(avans.BrojDokumenta)),
            new XElement(Cac + "TaxTotal", Amount(Cbc + "TaxAmount", pdvZaAvans), subtotali));
    }

    private static XElement BillingReference(FiltriranAvans avans) =>
        new(Cac + "BillingReference",
            new XElement(Cac + "InvoiceDocumentReference",
                new XElement(Cbc + "ID", Cln(avans.BrojDokumenta)),
                new XElement(Cbc + "IssueDate", avans.DatumIzdavanja?.ToString("yyyy-MM-dd") ?? "")));

    // Zbirno preko SVIH avansa: za svaku kategoriju glavne fakture, "šta ostaje"
    // posle odbitka iskorišćenih avansnih iznosa te iste kategorije. TaxSubtotal
    // se UVEK emituje za SVAKU kategoriju iz glavnog TaxTotal-a, čak i kad je
    // reduced=0 (u potpunosti pokriveno avansom) — SEF vraća grešku "TaxSubtotal
    // in extension is not defined" ako se izostavi, čak i kad je ukupno TaxAmount=0.
    private static XElement ReducedTotals(List<FiltriranAvans> avansi, EFakturaUblInput input,
        decimal osnovicaUkupno, decimal pdvUkupno, decimal ukupnoSaPdv, decimal prepaidAmount)
    {
        var iskorisceno = avansi
            .SelectMany(a => a.Kategorije)
            .GroupBy(k => (Id: AvansKategorijaId(k.Slovo), Procenat: k.Stopa))
            .ToDictionary(g => g.Key, g => (Osnovica: g.Sum(k => k.IskorisenaOsnovica), Pdv: g.Sum(k => k.IskorisenPdv)));

        var redovi = GrupisiGlavneKategorije(input)
            .Select(k =>
            {
                var iskor = iskorisceno.TryGetValue((k.Id, k.Procenat), out var v) ? v : (Osnovica: 0m, Pdv: 0m);
                return (k.Id, k.Procenat, ReducedOsnovica: k.Osnovica - iskor.Osnovica, ReducedPdv: k.Pdv - iskor.Pdv);
            })
            .ToList();

        var ukupanReducedPdv = redovi.Sum(r => r.ReducedPdv);

        var taxTotal = new XElement(Cac + "TaxTotal",
            Amount(Cbc + "TaxAmount", ukupanReducedPdv),
            redovi.Select(r => new XElement(Cac + "TaxSubtotal",
                Amount(Cbc + "TaxableAmount", r.ReducedOsnovica),
                Amount(Cbc + "TaxAmount", r.ReducedPdv),
                TaxCategory(input, r.Id, r.Procenat))));

        var legalMonetaryTotal = new XElement(Cac + "LegalMonetaryTotal",
            Amount(Cbc + "TaxExclusiveAmount", osnovicaUkupno),
            Amount(Cbc + "TaxInclusiveAmount", ukupnoSaPdv - prepaidAmount),
            Amount(Cbc + "PayableAmount", ukupnoSaPdv - prepaidAmount));

        return new XElement(Sbt + "ReducedTotals", taxTotal, legalMonetaryTotal);
    }

    // Smanjenje (CreditNote) koristi CreditNoteLine/CreditedQuantity umesto
    // InvoiceLine/InvoicedQuantity — sve ostalo unutar linije identično kao FAKTURA.
    private static XElement InvoiceLine(EFakturaUblStavka s, int rb, bool jeAvans, bool jeSmanjenje)
    {
        var baznaVrednost = s.Cena * s.Kolicina;

        var linija = new XElement(Cac + (jeSmanjenje ? "CreditNoteLine" : "InvoiceLine"),
            new XElement(Cbc + "ID", rb),
            new XElement(Cbc + (jeSmanjenje ? "CreditedQuantity" : "InvoicedQuantity"), new XAttribute("unitCode", MapUnitCode(s.Jm)), F4(s.Kolicina)),
            Amount(Cbc + "LineExtensionAmount", s.Osnovica));

        if (s.Umanjenje > 0)
        {
            var procenatRabata = baznaVrednost == 0 ? 0m : s.Umanjenje / baznaVrednost * 100m;
            linija.Add(new XElement(Cac + "AllowanceCharge",
                new XElement(Cbc + "ChargeIndicator", "false"),
                new XElement(Cbc + "MultiplierFactorNumeric", F2(procenatRabata)),
                Amount(Cbc + "Amount", s.Umanjenje),
                Amount(Cbc + "BaseAmount", baznaVrednost)));
        }

        var (kategorijaId, procenat) = KategorijaZaStavku(s);

        // BEZ TaxExemptionReasonCode ovde — ReasonCode ide SAMO u TaxTotal (po grupi).
        linija.Add(new XElement(Cac + "Item",
            new XElement(Cbc + "Name", Cln(s.Naziv)),
            RefTag(Cac + "SellersItemIdentification", s.Sifra),
            new XElement(Cac + "ClassifiedTaxCategory",
                new XElement(Cbc + "ID", kategorijaId),
                new XElement(Cbc + "Percent", F2(procenat)),
                new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT")))));

        // Avans: Cena na formi je "Iznos uplate" (bruto), ne jedinična neto cena —
        // Price/PriceAmount uzima Osnovicu (količina je uvek 1, pa je Osnovica i
        // jedinična cena). FAKTURA: Cena je već neto jedinična cena, bez izmene.
        var cenaZaPrikaz = jeAvans ? s.Osnovica : s.Cena;
        linija.Add(new XElement(Cac + "Price", Amount(Cbc + "PriceAmount", cenaZaPrikaz)));

        return linija;
    }

    // Matični broj sa 7 cifara dobija vodeću nulu (SEF očekuje 8 cifara).
    private static string PadMaticni(string? maticniBroj)
    {
        var m = Cln(maticniBroj);
        return m.Length == 7 && m.All(char.IsDigit) ? "0" + m : m;
    }

    // "dan"/"d" oba mapiraju na DAY — "dan" je vrednost koju koristi naš JM
    // padajući meni na formi, "d" je skraćenica iz opšte mapping tabele.
    private static string MapUnitCode(string? jm) => (jm ?? "").Trim().ToLowerInvariant() switch
    {
        "kom"          => "H87",
        "kg"           => "KGM",
        "km"           => "KMT",
        "g"            => "GRM",
        "m"            => "MTR",
        "l"            => "LTR",
        "t" or "tona"  => "TNE",
        "m2"           => "MTK",
        "m3"           => "MTQ",
        "min"          => "MIN",
        "h"            => "HUR",
        "d" or "dan"   => "DAY",
        "kwh"          => "KWH",
        _              => "H87"
    };
}
