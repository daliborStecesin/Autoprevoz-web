using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_plate")]
public class Plata
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    public int VozacId { get; set; }

    public DateTime Datum { get; set; }

    [MaxLength(20)]
    public string TipIsplate { get; set; } = "FIXNO";

    public decimal Kolicina { get; set; }

    public decimal Cena { get; set; }

    public decimal Iznos { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Neplaceno";

    public DateTime? DatumIsplate { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    [ForeignKey("VozacId")]
    public virtual Vozac? Vozac { get; set; }
}
