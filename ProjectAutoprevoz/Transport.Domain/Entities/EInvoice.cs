namespace Transport.Domain.Entities;

public class EInvoice
{
    public int idEfakture { get; set; }

    public int? idRacuna { get; set; }
    public string? tipPrimaoca { get; set; }
    public string? tipFakture { get; set; }
    public string? tipDokumenta { get; set; }
    public string? brojDokumenta { get; set; }
    public int? idPartnera { get; set; }
    public string? partner { get; set; }
    public string? pib { get; set; }
    public int? sendInvoiceToCir { get; set; }
    public string? ugovorBr { get; set; }
    public string? porudzbinaBr { get; set; }
    public string? tenderBr { get; set; }
    public string? CRFidentifikator { get; set; }
    public string? CRF_Status { get; set; }
    public string? statusDokumenta { get; set; }
    public string? statusPlacanja { get; set; }
    public string? PDV_dospece { get; set; }
    public int? idPoreskoOslobodjenje { get; set; }
    public string? clanPoreskogOslobodjenje { get; set; }
    public string? prilog { get; set; }
    public string? invoiceID { get; set; }
    public string? salesInvoiceID { get; set; }
    public string? purchaseInvoiceId { get; set; }
    public string? cirID { get; set; }
    public DateTime? vremeSlanja { get; set; }
    public decimal? kurs { get; set; }
    public string? valuta { get; set; }
    public int? avansi { get; set; }
    public int? pratecaDokumenta { get; set; }
    public string? invoiceMessage { get; set; }
    public string? acceptRejectMessage { get; set; }
    public string? cancelInvoiceMessage { get; set; }
    public string? prepaymentInvoiceNumber { get; set; }
    public string? komentar { get; set; }
    public int? vatPointDate { get; set; }
    public DateTime? accountingDateUtc { get; set; }
    public DateTime? paymentDateUtc { get; set; }
    public DateTime? invoiceDateUtc { get; set; }
    public DateTime? invoiceSentDateUtc { get; set; }
    public decimal? totalToPay { get; set; }
    public decimal? discountPercentage { get; set; }
    public decimal? discountAmount { get; set; }
    public decimal? sumWithoutVat { get; set; }
    public decimal? vatRate { get; set; }
    public decimal? vatSum { get; set; }
    public decimal? sumWithVat { get; set; }
    public string? model { get; set; }
    public string? pozivNaBroj { get; set; }
    public string? sourceInvoiceSelectionMode { get; set; }
    public DateTime? indebtednessPeriodFromDate { get; set; }
    public DateTime? indebtednessPeriodToDate { get; set; }
    public int? nijeSaSef { get; set; }
    public string? sourceInvoices { get; set; }
    public string? korisnik { get; set; }
    public string? status { get; set; }
    public long? invoiceIDint { get; set; }
}
