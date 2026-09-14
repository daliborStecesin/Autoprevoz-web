namespace Transport.Web.Components.Shared;

// Jedno mesto za sve dvojezične (sr/en) labele na štampama računa/ponuda/
// predračuna. DokumentStampa.razor čita odavde. FakturaStampa.razor NIJE
// prebačena na ovo (namerno, v214 zadatak) — ali su ključevi/vrednosti
// prepisani REČ ZA REČ iz njenih trenutnih hardkodovanih stringova, tako
// da prelazak kasnije ne menja ništa vizuelno.
public enum NazivKljuc
{
    Primalac,
    DatumIzdavanja,
    MestoIzdavanja,
    DatumPrometa,

    TabelaRedniBroj,
    TabelaOpis,
    TabelaJm,
    TabelaKolicina,
    TabelaCena,
    TabelaRabat,
    TabelaCenaSaRabatom,
    TabelaOsnovica,
    TabelaPdvProcenat,

    NapomenaOslobodjenje,
    ValutaPlacanja,
    VaziDo,

    RekapOsnovica,
    RekapPdv,
    RekapUkupno,
    KursEur,
    VrednostRsd,

    DevizniRacun,
    Banka,

    IzdaoDokument,
    Potpis,

    NaslovRacun,
    NaslovPonuda,
    NaslovPredracun,
}

public static class NaziviDokumenata
{
    private static readonly Dictionary<NazivKljuc, (string Sr, string En)> _nazivi = new()
    {
        [NazivKljuc.Primalac]              = ("Primalac", "Bill To"),
        [NazivKljuc.DatumIzdavanja]         = ("Datum izdavanja računa:", "Date of invoice:"),
        [NazivKljuc.MestoIzdavanja]         = ("Mesto izdavanja:", "Place of invoice:"),
        [NazivKljuc.DatumPrometa]           = ("Datum prometa usluge:", "Date of supply:"),

        [NazivKljuc.TabelaRedniBroj]        = ("R.Br", "Id."),
        [NazivKljuc.TabelaOpis]             = ("Vrsta usluge", "Product / Service"),
        [NazivKljuc.TabelaJm]               = ("JM", "UoM"),
        [NazivKljuc.TabelaKolicina]         = ("Kol.", "QTY"),
        [NazivKljuc.TabelaCena]             = ("Cena", "Unit Price"),
        [NazivKljuc.TabelaRabat]            = ("Rbt%", "Disc%"),
        [NazivKljuc.TabelaCenaSaRabatom]    = ("Cena sa Rbt", "Net Price"),
        [NazivKljuc.TabelaOsnovica]         = ("Osnovica", "Base"),
        [NazivKljuc.TabelaPdvProcenat]      = ("PDV %", "VAT%"),

        [NazivKljuc.NapomenaOslobodjenje]   = ("Napomena o poreskom oslobođenju:", "Tax exemption note:"),
        [NazivKljuc.ValutaPlacanja]         = ("Valuta plaćanja:", "Payment date:"),
        [NazivKljuc.VaziDo]                 = ("Važi do:", "Valid until:"),

        [NazivKljuc.RekapOsnovica]          = ("Osnovica:", "TOTAL (EUR):"),
        [NazivKljuc.RekapPdv]               = ("Iznos PDV:", "TAX (EUR):"),
        [NazivKljuc.RekapUkupno]            = ("UKUPNO:", "FOR PAYMENT (EUR):"),
        [NazivKljuc.KursEur]                = ("Kurs EUR:", "EUR rate:"),
        [NazivKljuc.VrednostRsd]            = ("Vrednost RSD:", "Value RSD:"),

        [NazivKljuc.DevizniRacun]           = ("Devizni račun:", "Bank account:"),
        [NazivKljuc.Banka]                  = ("Banka:", "Bank:"),

        [NazivKljuc.IzdaoDokument]          = ("Račun izdao:", "Invoice issued by:"),
        [NazivKljuc.Potpis]                 = ("Potpis", "Signature"),

        [NazivKljuc.NaslovRacun]            = ("RAČUN broj", "Invoice no"),
        [NazivKljuc.NaslovPonuda]           = ("PONUDA broj", "QUOTATION no"),
        [NazivKljuc.NaslovPredracun]        = ("PREDRAČUN broj", "PROFORMA INVOICE no"),
    };

    public static string Naziv(NazivKljuc kljuc, bool eng)
    {
        var (sr, en) = _nazivi[kljuc];
        return eng ? en : sr;
    }
}
