using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

/// <summary>Telo za POST purchase-invoice/acceptRejectPurchaseInvoice.</summary>
public class AcceptRejectRequestDto
{
    [JsonPropertyName("invoiceId")]
    public long InvoiceId { get; set; }

    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = "";
}
