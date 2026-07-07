namespace Transport.Domain.Entities;

public class LineItem
{
    public int idKolone { get; set; }

    public int? idRacuna { get; set; }
    public long? rowId { get; set; }
    public long? invoiceId { get; set; }
    public int? orderNo { get; set; }
    public string? code { get; set; }
    public string? description { get; set; }
    public string? unit { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? quantity { get; set; }
    public decimal? discountPercentage { get; set; }
    public decimal? discountAmount { get; set; }
    public decimal? sumWithoutVat { get; set; }
    public decimal? vatRate { get; set; }
    public decimal? vatSum { get; set; }
    public decimal? sumWithVat { get; set; }
    public string? vatCategoryCode { get; set; }
    public string? tipRacuna { get; set; }
    public decimal? cenaSP { get; set; }
    public decimal? cenaSaRbt { get; set; }
    public string? KeyClan { get; set; }
    public int? idTaxExemption { get; set; }
}
