using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_vozila")]
public class Vozilo
{
    [Key]
    [Column("VoziloId")]
    public int VoziloId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RegistrskaBroja { get; set; } = null!;

    [MaxLength(100)]
    public string? Marka { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    public int? GodinaProizvodnje { get; set; }

    [MaxLength(20)]
    public string? VinBroj { get; set; }

    [MaxLength(20)]
    public string? VrstaGoriva { get; set; }

    public decimal? KapacitetTerete { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Aktivno";

    public DateTime? DatumRegistracije { get; set; }

    public DateTime? DatumTehnickog { get; set; }

    public DateTime? DatumOsiguranja { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<NalogPrevoz> Nalozi { get; set; } = [];
}
