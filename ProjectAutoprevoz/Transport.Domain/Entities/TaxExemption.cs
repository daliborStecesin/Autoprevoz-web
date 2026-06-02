using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_taxExemptionList")]
public class TaxExemption
{
    [Key]
    [Column("idTaxExemption")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTaxExemption { get; set; }

    [Column("reasonId")]
    public int? ReasonId { get; set; }

    [Column("KeyClan")]   [MaxLength(50)]  public string? KeyClan      { get; set; }
    [Column("law")]       [MaxLength(250)] public string? Law          { get; set; }
    [Column("article")]   [MaxLength(10)]  public string? Article      { get; set; }
    [Column("paragraph")] [MaxLength(10)]  public string? Paragraph    { get; set; }
    [Column("point")]     [MaxLength(10)]  public string? Point        { get; set; }
    [Column("subpoint")]  [MaxLength(10)]  public string? Subpoint     { get; set; }
    [Column("text")]                       public string? Text         { get; set; }
    [Column("freeFormNote")]               public string? FreeFormNote { get; set; }
    [Column("activeFrom")]                 public DateTime? ActiveFrom { get; set; }
    [Column("activeTo")]                   public DateTime? ActiveTo   { get; set; }
    [Column("category")]  [MaxLength(10)]  public string? Category     { get; set; }
    [Column("Koristi")]                    public int?    Koristi      { get; set; }
}
