using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_log_brisanja")]
public class LogBrisanja
{
    [Key]
    public int Id { get; set; }

    public DateTime datumVreme { get; set; } = DateTime.Now;

    public int? idKorisnika { get; set; }

    [MaxLength(70)]
    public string? imeKorisnika { get; set; }

    [MaxLength(50)]
    public string? formaModul { get; set; }

    [MaxLength(500)]
    public string? opis { get; set; }
}
