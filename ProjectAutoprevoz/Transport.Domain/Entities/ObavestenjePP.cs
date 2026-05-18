using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_ObavestenjaPP")]
public class ObavestenjePP
{
    [Key]
    [Column("ObavestenjeID")]
    public int ObavestenjeId { get; set; }

    public DateTime Datum { get; set; }

    [MaxLength(100)]
    public string? Naslov { get; set; }

    [MaxLength(500)]
    public string? Opis { get; set; }

    [Column("statust")]
    [MaxLength(20)]
    public string Status { get; set; } = "Novo";

    [MaxLength(100)]
    public string? Referenca { get; set; }

    [MaxLength(500)]
    public string? Akcija { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
