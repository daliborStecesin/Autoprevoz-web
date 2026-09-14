namespace Transport.Web.Components.Shared;

// Jedno mesto za sve dvojezične (sr/en) labele na štampama računa/ponuda/
// predračuna. Čitaju odavde i FakturaStampa.razor i DokumentStampa.razor.
public enum NazivKljuc
{
    Primalac,
    DatumIzdavanja,
    MestoIzdavanja,
    DatumPrometa,
    DatumUtovara,
    NalogUtovar,
    Vozilo,

    Pib,
    MaticniBroj,
    Telefon,
    Mobilni,
    Pecat,

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

    // Naslov taba / ime PDF fajla (bez dijakritika — to je ime pod kojim
    // primalac snima fajl na disk).
    NaslovFajlaRacun,
    NaslovFajlaPonuda,
    NaslovFajlaPredracun,
}

public static class NaziviDokumenata
{
    private static readonly Dictionary<NazivKljuc, (string Sr, string En)> _nazivi = new()
    {
        [NazivKljuc.Primalac]              = ("Primalac", "Bill To"),
        [NazivKljuc.DatumIzdavanja]         = ("Datum izdavanja računa:", "Date of invoice:"),
        [NazivKljuc.MestoIzdavanja]         = ("Mesto izdavanja:", "Place of invoice:"),
        [NazivKljuc.DatumPrometa]           = ("Datum prometa usluge:", "Date of supply:"),
        [NazivKljuc.DatumUtovara]           = ("Datum utovara:", "Loading date:"),
        [NazivKljuc.NalogUtovar]            = ("Nalog za utovar:", "Order number:"),
        [NazivKljuc.Vozilo]                 = ("Transport dobara izvršen vozilom:", "Truck registration no:"),

        [NazivKljuc.Pib]                    = ("PIB:", "VAT No.:"),
        [NazivKljuc.MaticniBroj]            = ("Matični broj:", "Company Reg. No.:"),
        [NazivKljuc.Telefon]                = ("Tel:", "Tel:"),
        [NazivKljuc.Mobilni]                = ("Mob:", "Mobile:"),
        [NazivKljuc.Pecat]                  = ("M.P.", "Stamp"),

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

        [NazivKljuc.NaslovFajlaRacun]       = ("Racun", "Invoice"),
        [NazivKljuc.NaslovFajlaPonuda]      = ("Ponuda", "Quotation"),
        [NazivKljuc.NaslovFajlaPredracun]   = ("Predracun", "Proforma"),
    };

    public static string Naziv(NazivKljuc kljuc, bool eng)
    {
        var (sr, en) = _nazivi[kljuc];
        return eng ? en : sr;
    }
}
