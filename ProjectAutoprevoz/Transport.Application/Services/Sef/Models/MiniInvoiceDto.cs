using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

public class MiniInvoiceDto
{
    [JsonPropertyName("InvoiceId")]
    public long? InvoiceId { get; set; }

    [JsonPropertyName("PurchaseInvoiceId")]
    public long? PurchaseInvoiceId { get; set; }

    [JsonPropertyName("SalesInvoiceId")]
    public long? SalesInvoiceId { get; set; }
}
