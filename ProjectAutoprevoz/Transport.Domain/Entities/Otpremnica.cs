using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_otpremnice")]
public class Otpremnica
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Required]
    [MaxLength(20)]
    public string BrojOtpremnice { get; set; } = null!;

    public int? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? PartnerNaziv { get; set; }

    public DateTime DatumDokumenta { get; set; }

    [MaxLength(200)]
    public string? Opis { get; set; }

    public decimal BrutoIznos { get; set; }

    public decimal PDV { get; set; }

    public decimal Ukupno { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Otvoren";

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public int? Brisano { get; set; } = 0;

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<StavkaOtpremnice> Stavke { get; set; } = [];
}
