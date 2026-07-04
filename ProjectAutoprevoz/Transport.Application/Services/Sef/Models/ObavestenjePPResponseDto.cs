using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

/// <summary>Odgovor SEF-a za recipients-notice-on-input-vat (GET liste i POST /sender/send).</summary>
public class ObavestenjePPResponseDto
{
    [JsonPropertyName("Id")]
    public long Id { get; set; }

    [JsonPropertyName("NoticeNumber")]
    public string? NoticeNumber { get; set; }

    [JsonPropertyName("NoticeDate")]
    public DateTime? NoticeDate { get; set; }

    [JsonPropertyName("TotalVatAmount")]
    public decimal? TotalVatAmount { get; set; }

    [JsonPropertyName("RelatedDocumentNumber")]
    public string? RelatedDocumentNumber { get; set; }

    [JsonPropertyName("SendingStatus")]
    public string? SendingStatus { get; set; }

    [JsonPropertyName("Sender")]
    public ObavestenjePPPartyDto? Sender { get; set; }

    [JsonPropertyName("Recipient")]
    public ObavestenjePPPartyDto? Recipient { get; set; }
}

public class ObavestenjePPPartyDto
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("VatRegistrationCode")]
    public string? VatRegistrationCode { get; set; }

    [JsonPropertyName("RegistrationCode")]
    public string? RegistrationCode { get; set; }
}
