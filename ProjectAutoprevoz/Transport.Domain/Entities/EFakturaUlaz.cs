namespace Transport.Domain.Entities;

public class EFakturaUlaz
{
    public int idEfakture { get; set; }

    public int? idRacuna { get; set; }
    public int? idPartnera { get; set; }
    public string? naziv { get; set; }
    public string? PIB { get; set; }
    public string? MB { get; set; }
    public string? tipPrimaoca { get; set; }
    public string? tipFakture { get; set; }
    public string? tipDokumenta { get; set; }
    public DateTime? invoiceSentDateUtc { get; set; }
    public DateTime? accountingDateUtc { get; set; }
    public DateTime? invoiceDateUtc { get; set; }
    public DateTime? paymentDateUtc { get; set; }
    public decimal? Vrednost { get; set; }
    public decimal? Rabat { get; set; }
    public decimal? Osnovica { get; set; }
    public decimal? PDV { get; set; }
    public decimal? Ukupno { get; set; }
    public string? ugovorBr { get; set; }
    public string? porudzbinaBr { get; set; }
    public string? tenderBr { get; set; }
    public string? CRFidentifikator { get; set; }
    public string? CRF_Status { get; set; }
    public string? statusDokumenta { get; set; }
    public string? statusDokumentaDobavljaca { get; set; }
    public string? statusPlacanja { get; set; }
    public string? PDV_dospece { get; set; }
    public int? idPoreskoOslobodjenje { get; set; }
    public string? prilog { get; set; }
    public string? invoiceID { get; set; }
    public string? salesInvoiceID { get; set; }
    public string? referenceNumber { get; set; }
    public string? modelNumber { get; set; }
    public string? purchaseInvoiceId { get; set; }
    public string? cirID { get; set; }
    public string? description { get; set; }
    public string? note { get; set; }
    public string? cancelInvoiceMessage { get; set; }
    public string? acceptRejectMessage { get; set; }
    public string? invoiceFilePath { get; set; }
    public string? brojDokumenta { get; set; }
    public long? invoiceIDint { get; set; }
}
