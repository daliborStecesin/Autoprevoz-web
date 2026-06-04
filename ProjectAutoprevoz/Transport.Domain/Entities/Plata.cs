using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_plate")]
public class Plata
{
    [Key]
    [Column("plataId")]
    public int PlataId { get; set; }

    [MaxLength(10)]
    public string? idVozaca { get; set; }

    [MaxLength(10)]
    public string? idVozila { get; set; }

    [MaxLength(100)]
    public string? tura { get; set; }

    public decimal? vrednostTure { get; set; }
    public decimal? avans { get; set; }
    public decimal? dnevnica { get; set; }
    public decimal? procenat { get; set; }
    public decimal? vracenAvans { get; set; }
    public decimal? iznosPlate { get; set; }
    public decimal? zaIsplatu { get; set; }

    [MaxLength(100)]
    public string? Napomena { get; set; }

    [Column(TypeName = "date")]
    public DateTime? datum { get; set; }

    [MaxLength(20)]
    public string? tipIsplate { get; set; }

    public decimal? km { get; set; }
    public decimal? cenaPoKm { get; set; }
    public decimal? fixnoPlata { get; set; }

    [MaxLength(20)]
    public string? izvorObracuna { get; set; }

    public int isplaceno { get; set; } = 0;

    [Column(TypeName = "date")]
    public DateTime? datumIsplate { get; set; }

    [MaxLength(100)]
    public string? uneo { get; set; }

    public DateTime? datumUnosa { get; set; }

    public int brisano { get; set; } = 0;

    public int?      Izmenio     { get; set; }
    public DateTime? DatumIzmene { get; set; }
}
