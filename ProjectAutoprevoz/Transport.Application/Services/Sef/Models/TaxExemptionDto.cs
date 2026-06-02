using System.Text.Json.Serialization;

namespace Transport.Application.Services.Sef.Models;

public class TaxExemptionDto
{
    [JsonPropertyName("reasonId")]   public int      ReasonId     { get; set; }
    [JsonPropertyName("key")]        public string?  Key          { get; set; }
    [JsonPropertyName("law")]        public string?  Law          { get; set; }
    [JsonPropertyName("article")]    public string?  Article      { get; set; }
    [JsonPropertyName("paragraph")]  public string?  Paragraph    { get; set; }
    [JsonPropertyName("point")]      public string?  Point        { get; set; }
    [JsonPropertyName("subpoint")]   public string?  Subpoint     { get; set; }
    [JsonPropertyName("text")]       public string?  Text         { get; set; }
    [JsonPropertyName("freeFormNote")] public string? FreeFormNote { get; set; }
    [JsonPropertyName("activeFrom")] public DateTime? ActiveFrom  { get; set; }
    [JsonPropertyName("activeTo")]   public DateTime? ActiveTo    { get; set; }
    [JsonPropertyName("category")]   public string?  Category     { get; set; }
}
