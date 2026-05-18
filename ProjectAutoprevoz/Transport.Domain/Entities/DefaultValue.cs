using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_DefaultValues")]
public class DefaultValue
{
    [Key]
    [Column("DefaultId")]
    public int DefaultId { get; set; }

    [MaxLength(50)]
    public string? Forma { get; set; }

    [MaxLength(50)]
    public string? Kontrola { get; set; }

    [MaxLength(100)]
    public string? Korisnik { get; set; }

    [MaxLength(500)]
    public string? Vrednost { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
