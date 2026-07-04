namespace Transport.Domain.Entities;

public class IndividualVatRecord
{
    public int idUnosa { get; set; }

    public long? idIndividualVat { get; set; }

    public int? year { get; set; }

    public string? calculationNumber { get; set; }

    public string? documentNumber { get; set; }

    public string? pibPartnera { get; set; }

    public string? vatPeriodStr { get; set; }

    public string? documentDirectionStr { get; set; }

    public string? documentType { get; set; }

    public int? internalInvoiceOption { get; set; }

    public string? relatedPartyIdentifier { get; set; }

    public string? internalInvoiceNumber { get; set; }

    public string? basisForPrepayment { get; set; }

    public DateTime? recordingDate { get; set; }

    public DateTime? statusChangeDate { get; set; }

    public string? status { get; set; }

    public decimal? totalCalculatedVat { get; set; }
}
