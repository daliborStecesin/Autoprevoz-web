using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

/// <summary>Telo za POST recipients-notice-on-input-vat/sender/send.</summary>
public class ObavestenjePPSendDto
{
    [JsonPropertyName("noticeNumber")]
    public string NoticeNumber { get; set; } = "";

    [JsonPropertyName("totalVatAmount")]
    public decimal TotalVatAmount { get; set; }

    [JsonPropertyName("documentIssueOrigin")]
    public string DocumentIssueOrigin { get; set; } = "";

    [JsonPropertyName("basisOfNotice")]
    public string BasisOfNotice { get; set; } = "";

    [JsonPropertyName("documentReferenceType")]
    public string DocumentReferenceType { get; set; } = "";

    [JsonPropertyName("relatedDocumentNumber")]
    public string RelatedDocumentNumber { get; set; } = "";

    [JsonPropertyName("relatedDocumentIssueDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RelatedDocumentIssueDate { get; set; }

    [JsonPropertyName("relatedInvoicePeriodStartDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RelatedInvoicePeriodStartDate { get; set; }

    [JsonPropertyName("relatedInvoicePeriodEndDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RelatedInvoicePeriodEndDate { get; set; }

    [JsonPropertyName("relatedDocumentStornoDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RelatedDocumentStornoDate { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = "";

    [JsonPropertyName("recipient")]
    public ObavestenjePPRecipientDto Recipient { get; set; } = new();
}

public class ObavestenjePPRecipientDto
{
    [JsonPropertyName("vatRegistrationCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VatRegistrationCode { get; set; }

    [JsonPropertyName("registrationCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RegistrationCode { get; set; }
}
