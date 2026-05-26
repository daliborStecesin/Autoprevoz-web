using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_dnevnice")]
public class Dnevnica
{
    [Key]
    [Column("idDnevnica")]
    public int IdDnevnica { get; set; }

    [MaxLength(100)]
    public string? brNaloga { get; set; }

    [MaxLength(20)]
    public string? tipDnevnice { get; set; }

    [MaxLength(100)]
    public string? vozac { get; set; }

    [MaxLength(20)]
    public string? idVozaca { get; set; }

    [MaxLength(100)]
    public string? vozilo { get; set; }

    [MaxLength(20)]
    public string? idVozila { get; set; }

    [MaxLength(50)]
    public string? regOznaka { get; set; }

    [MaxLength(100)]
    public string? zemlja { get; set; }

    public DateTime? datumPolaska { get; set; }

    public DateTime? datumDolaska { get; set; }

    public decimal? brCasova { get; set; }

    public decimal? brojDnevnica { get; set; }

    public decimal? dnevnica { get; set; }

    public decimal? Ukupno { get; set; }

    [MaxLength(500)]
    public string? relacija { get; set; }

    [Column(TypeName = "date")]
    public DateTime? datumDokumenta { get; set; }

    public DateTime? datumIzlaska { get; set; }

    public DateTime? datumUlaska { get; set; }

    public int? zaIsplatu { get; set; }

    public int? isplaceno { get; set; } = 0;

    [Column(TypeName = "date")]
    public DateTime? datumIsplate { get; set; }

    public decimal? akontacija { get; set; }
}
