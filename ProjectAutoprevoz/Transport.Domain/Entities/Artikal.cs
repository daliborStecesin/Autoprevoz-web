using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_lager")]
public class Artikal
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Required]
    [MaxLength(100)]
    public string Naziv { get; set; } = null!;

    [MaxLength(50)]
    public string? Sifra { get; set; }

    [MaxLength(20)]
    public string? Barcode { get; set; }

    public decimal Kolicina { get; set; }

    [MaxLength(20)]
    public string Jedinica { get; set; } = "kom";

    public decimal CenaOsnovnu { get; set; }

    public decimal CenaProdajnu { get; set; }

    public decimal PDV { get; set; } = 20.0m;

    [MaxLength(100)]
    public string? Kategorija { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Aktivan";

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
