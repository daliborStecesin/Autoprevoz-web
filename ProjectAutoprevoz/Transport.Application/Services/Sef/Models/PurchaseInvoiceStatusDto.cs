using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

public class PurchaseInvoiceStatusDto
{
    [JsonPropertyName("InvoiceId")]
    public long InvoiceId { get; set; }

    [JsonPropertyName("GlobUniqId")]
    public string? GlobUniqId { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("CirStatus")]
    public string? CirStatus { get; set; }

    [JsonPropertyName("CirInvoiceId")]
    public string? CirInvoiceId { get; set; }

    [JsonPropertyName("Version")]
    public int? Version { get; set; }

    [JsonPropertyName("LastModifiedUtc")]
    public DateTime? LastModifiedUtc { get; set; }

    [JsonPropertyName("CirSettledAmount")]
    public decimal? CirSettledAmount { get; set; }
}
