using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Transport.Application.Services.Sef.Models;

namespace Transport.Application.Services.Sef;

// Gradi UBL XML za minimalnu FAKTURU (S10/S20), redosled elemenata prati
// desktop Class_E_Racun (već prihvaćen na SEF-u). Bez avansa/oslobođenja/
// knjižnih/priloga — dodaju se u kasnijim koracima. Čist string builder,
// bez baze i bez slanja. XElement escapuje &,<,> automatski u tekstu.
public class EFakturaUblBuilder : IEFakturaUblBuilder
{
    private static readonly XNamespace Ns  = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace Cec = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private static readonly XNamespace Xsd = "http://www.w3.org/2001/XMLSchema";
    private static readonly XNamespace Sbt = "http://mfin.gov.rs/srbdt/srbdtext";

    public string Build(EFakturaUblInput input)
    {
        var osnovicaUkupno = input.Stavke.Sum(s => s.Osnovica);
        var pdvUkupno       = input.Stavke.Sum(s => s.Pdv);
        var ukupnoSaPdv     = osnovicaUkupno + pdvUkupno;

        var invoice = new XElement(Ns + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cec", Cec.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "cac", Cac.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "cbc", Cbc.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "xsd", Xsd.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "sbt", Sbt.NamespaceName),

            new XElement(Cbc + "CustomizationID", "urn:cen.eu:en16931:2017#compliant#urn:mfin.gov.rs:srbdt:2022"),
            new XElement(Cbc + "ID", Cln(input.BrojDokumenta)),
            new XElement(Cbc + "IssueDate", DateTime.Today.ToString("yyyy-MM-dd")),
            Tag(Cbc + "DueDate", input.DatumValute?.ToString("yyyy-MM-dd")),
            new XElement(Cbc + "InvoiceTypeCode", "380"),
            NoteTag(input),
            new XElement(Cbc + "DocumentCurrencyCode", "RSD"),
            Tag(Cbc + "BuyerReference", input.InterniBrojZaRutiranje),

            InvoicePeriod(input.NastanakPdvObaveze),
            RefTag(Cac + "OrderReference", input.BrojNarudzbenice),
            RefTag(Cac + "OriginatorDocumentReference", input.BrojTendera),
            RefTag(Cac + "ContractDocumentReference", input.BrojUgovora),

            AccountingSupplierParty(input),
            AccountingCustomerParty(input),

            Delivery(input.DatumPrometa),
            PaymentMeans(input),
            TaxTotal(input, pdvUkupno),
            LegalMonetaryTotal(osnovicaUkupno, ukupnoSaPdv),

            input.Stavke.Select((s, i) => InvoiceLine(s, i + 1))
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

    // Note = "Vozilo: {vozilo} CMR: {cmr} {komentar}" — ceo tag izostavljen samo
    // ako su sva tri izvorna polja prazna (ne trimuje pojedinačne praznine, samo
    // vodeći/prateći razmak cele vrednosti kao i svaki drugi tekst).
    private static XElement? NoteTag(EFakturaUblInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Vozilo) &&
            string.IsNullOrWhiteSpace(input.BrojCmr) &&
            string.IsNullOrWhiteSpace(input.Komentar))
            return null;

        return new XElement(Cbc + "Note", Cln($"Vozilo: {input.Vozilo} CMR: {input.BrojCmr} {input.Komentar}"));
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

    private static XElement TaxTotal(EFakturaUblInput input, decimal pdvUkupno)
    {
        var subtotali = input.Stavke
            .GroupBy(s => s.PdvProcenat)
            .OrderByDescending(g => g.Key)
            .Select(g => new XElement(Cac + "TaxSubtotal",
                Amount(Cbc + "TaxableAmount", g.Sum(s => s.Osnovica)),
                Amount(Cbc + "TaxAmount", g.Sum(s => s.Pdv)),
                new XElement(Cac + "TaxCategory",
                    new XElement(Cbc + "ID", "S"),
                    new XElement(Cbc + "Percent", F2(g.Key)),
                    new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT")))));

        return new XElement(Cac + "TaxTotal", Amount(Cbc + "TaxAmount", pdvUkupno), subtotali);
    }

    private static XElement LegalMonetaryTotal(decimal osnovicaUkupno, decimal ukupnoSaPdv) =>
        new(Cac + "LegalMonetaryTotal",
            Amount(Cbc + "LineExtensionAmount", osnovicaUkupno),
            Amount(Cbc + "TaxExclusiveAmount", osnovicaUkupno),
            Amount(Cbc + "TaxInclusiveAmount", ukupnoSaPdv),
            Amount(Cbc + "AllowanceTotalAmount", 0m),
            Amount(Cbc + "PrepaidAmount", 0m),
            Amount(Cbc + "PayableAmount", ukupnoSaPdv));

    private static XElement InvoiceLine(EFakturaUblStavka s, int rb)
    {
        var baznaVrednost = s.Cena * s.Kolicina;

        var linija = new XElement(Cac + "InvoiceLine",
            new XElement(Cbc + "ID", rb),
            new XElement(Cbc + "InvoicedQuantity", new XAttribute("unitCode", MapUnitCode(s.Jm)), F4(s.Kolicina)),
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

        linija.Add(new XElement(Cac + "Item",
            new XElement(Cbc + "Name", Cln(s.Naziv)),
            RefTag(Cac + "SellersItemIdentification", s.Sifra),
            new XElement(Cac + "ClassifiedTaxCategory",
                new XElement(Cbc + "ID", "S"),
                new XElement(Cbc + "Percent", F2(s.PdvProcenat)),
                new XElement(Cac + "TaxScheme", new XElement(Cbc + "ID", "VAT")))));

        linija.Add(new XElement(Cac + "Price", Amount(Cbc + "PriceAmount", s.Cena)));

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
