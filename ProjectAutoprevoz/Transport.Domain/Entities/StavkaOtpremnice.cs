using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_artikli_otpremnice")]
public class StavkaOtpremnice
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    public int BrojOtpremnice { get; set; }

    [MaxLength(100)]
    public string? Opis { get; set; }

    public decimal Kolicina { get; set; } = 1.0m;

    [MaxLength(20)]
    public string Jedinica { get; set; } = "kom";

    public decimal JedinicnaCena { get; set; }

    public decimal Iznos { get; set; }

    public decimal StopaPDV { get; set; } = 20.0m;

    public decimal IznosPDV { get; set; }

    public decimal IznosUkupno { get; set; }

    [ForeignKey("BrojOtpremnice")]
    public virtual Otpremnica? Otpremnica { get; set; }
}
