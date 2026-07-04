using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

public class PurchaseInvoiceIdsDto
{
    [JsonPropertyName("PurchaseInvoiceIds")]
    public List<long> PurchaseInvoiceIds { get; set; } = [];
}
