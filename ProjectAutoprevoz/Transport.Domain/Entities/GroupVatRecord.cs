namespace Transport.Domain.Entities;

public class GroupVatRecord
{
    public int idZbirne { get; set; }

    public long? idGroupVat { get; set; }

    public int? year { get; set; }

    public string? calculationNumber { get; set; }

    public string? documentNumber { get; set; }

    public string? vatPeriodStr { get; set; }

    public string? relatedPartyIdentifier { get; set; }

    public DateTime? recordingDate { get; set; }

    public DateTime? statusChangeDate { get; set; }

    public string? vatRecordingStatus { get; set; }

    public DateTime? createdUtc { get; set; }
}
