using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

/// <summary>Telo za POST sales-invoice/storno.</summary>
public class StornoRequestDto
{
    [JsonPropertyName("invoiceId")]
    public long InvoiceId { get; set; }

    [JsonPropertyName("stornoComment")]
    public string StornoComment { get; set; } = "";
}
