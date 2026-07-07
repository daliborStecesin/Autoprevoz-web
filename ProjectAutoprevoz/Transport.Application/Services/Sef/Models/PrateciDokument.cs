namespace Transport.Application.Services.Sef.Models;

/// <summary>Jedan prateći dokument (prilog) izvučen iz cac:AdditionalDocumentReference.</summary>
public class PrateciDokument
{
    public string Naziv { get; set; } = "";
    public string Base64Sadrzaj { get; set; } = "";
}
