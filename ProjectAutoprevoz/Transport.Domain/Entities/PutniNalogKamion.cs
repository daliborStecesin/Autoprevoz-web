using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_putniNalogKamion")]
public class PutniNalogKamion
{
    [Key]
    [Column("IdNaloga")]
    public int IdNaloga { get; set; }

    [MaxLength(50)]
    public string? RegistrskaBroja { get; set; }

    public DateTime? DatumIzlaska { get; set; }

    public DateTime? DatumUlaska { get; set; }

    public decimal? PredjeniKm { get; set; }

    public decimal? PocetnoStanjeGoriva { get; set; }

    public decimal? KrajnjeStanjeGoriva { get; set; }

    public decimal? UkupnaPotrosnja { get; set; }

    public decimal? ProracunataGorivo { get; set; }

    [MaxLength(20)]
    public string VidGoriva { get; set; } = "Dizel";

    public decimal? CenaGoriva { get; set; }

    public decimal? IznosGoriva { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    [ForeignKey("IdNaloga")]
    public virtual NalogPrevoz? Nalog { get; set; }
}
