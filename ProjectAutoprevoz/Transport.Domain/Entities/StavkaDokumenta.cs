using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_artikli_dokumenta")]
public class StavkaDokumenta
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    public int IdDokumenta { get; set; }

    [Column("Id_Lager")]
    [MaxLength(5)]
    public string? IdLager { get; set; }

    [MaxLength(20)]
    public string? Barcode { get; set; }

    [MaxLength(2000)]
    public string? Artikal { get; set; }

    [MaxLength(15)]
    public string? JM { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Kolicina { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? CenaPoJMBP { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? CenaPoJMSP { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Rabat { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? CenaPoJMBPminusRab { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? VrednostMinusRab { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? StopaPDV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Osnovica { get; set; }

    [MaxLength(1)]
    public string? TipPDV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PDV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Ukupno { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Suma { get; set; }

    [ForeignKey("IdDokumenta")]
    public virtual Dokument? Dokument { get; set; }
}
