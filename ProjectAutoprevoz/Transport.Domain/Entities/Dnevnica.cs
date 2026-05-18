using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_dnevnice")]
public class Dnevnica
{
    [Key]
    [Column("IdDnevnica")]
    public int IdDnevnica { get; set; }

    public int VozacId { get; set; }

    public DateTime? DatumIzlaska { get; set; }

    public DateTime? DatumUlaska { get; set; }

    public int? BrojDana { get; set; }

    public decimal IznosPoDate { get; set; }

    public decimal UkupanIznos { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Nije isplaceno";

    public DateTime? DatumIsplate { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    [ForeignKey("VozacId")]
    public virtual Vozac? Vozac { get; set; }
}
