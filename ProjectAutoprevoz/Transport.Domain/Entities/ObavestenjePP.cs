namespace Transport.Domain.Entities;

public class ObavestenjePP
{
    public int ObavestenjeID { get; set; }

    public long? noticeId { get; set; }

    public string? noticeNumber { get; set; }

    public DateTime? NoticeDate { get; set; }

    public string? recipientPIB { get; set; }

    public string? recipientMB { get; set; }

    public decimal? totalVatAmount { get; set; }

    public string? Sender { get; set; }

    public string? tipSender { get; set; }

    public string? statust { get; set; }

    public int? senderId { get; set; }

    public string? documentNumber { get; set; }
}
