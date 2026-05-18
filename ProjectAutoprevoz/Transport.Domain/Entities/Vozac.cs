using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_imenik")]
public class Vozac
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Ime { get; set; } = null!;

    [MaxLength(100)]
    public string? Prezime { get; set; }

    [MaxLength(20)]
    public string? JMB { get; set; }

    [MaxLength(100)]
    public string? Mesto { get; set; }

    [MaxLength(100)]
    public string? Adresa { get; set; }

    [MaxLength(20)]
    public string? Telefon { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? BrojVozackeDozvole { get; set; }

    public DateTime? DatumIstekVozackeDozvole { get; set; }

    [MaxLength(20)]
    public string? KategorijaDozvoле { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Aktivan";

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<Dnevnica> Dnevnice { get; set; } = [];
    public virtual ICollection<Plata> Plate { get; set; } = [];
}
