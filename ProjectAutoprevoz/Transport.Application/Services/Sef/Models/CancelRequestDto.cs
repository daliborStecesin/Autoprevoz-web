using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

/// <summary>Telo za POST sales-invoice/cancel.</summary>
public class CancelRequestDto
{
    [JsonPropertyName("invoiceId")]
    public long InvoiceId { get; set; }

    [JsonPropertyName("cancelComments")]
    public string CancelComments { get; set; } = "";
}
