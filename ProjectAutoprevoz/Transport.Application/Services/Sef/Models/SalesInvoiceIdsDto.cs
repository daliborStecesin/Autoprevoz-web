using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

public class SalesInvoiceIdsDto
{
    [JsonPropertyName("SalesInvoiceIds")]
    public List<long> SalesInvoiceIds { get; set; } = [];
}
