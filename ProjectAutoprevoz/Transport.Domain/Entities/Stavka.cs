using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_artikli_racuna")]
public class Stavka
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Column("Id_Lager")]
    [MaxLength(5)]
    public string? IdLager { get; set; }

    [MaxLength(20)]
    public string? Barcode { get; set; }

    [MaxLength(2000)]
    public string? Artikal { get; set; }

    [MaxLength(15)]
    public string? JM { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Kolicina { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal? CenaPoJMBP { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CenaPoJMSP { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Rabat { get; set; }

    [Column(TypeName = "decimal(18,3)")]
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

    [MaxLength(15)]
    public string? selektor { get; set; }

    [MaxLength(15)]
    public string? Id_Racuna { get; set; }

    [MaxLength(15)]
    public string? Id_Partnera { get; set; }

    public DateTime? Datum { get; set; }

    [MaxLength(15)]
    public string? Status { get; set; }

    [MaxLength(15)]
    public string? Tip_Prodaje { get; set; }

    [MaxLength(15)]
    public string? Bon { get; set; }

    [MaxLength(3)]
    public string? Korisnik_Id { get; set; }

    [MaxLength(15)]
    public string? Vrsta_Placanja { get; set; }

    public int? idVozila { get; set; }

    [MaxLength(50)]
    public string? referenca1 { get; set; }

    [MaxLength(50)]
    public string? referenca2 { get; set; }

    [MaxLength(50)]
    public string? vozilo { get; set; }

    public DateTime? datumIstovara { get; set; }

    public DateTime? datumUtovara { get; set; }

    public int brisano { get; set; } = 0;
}
