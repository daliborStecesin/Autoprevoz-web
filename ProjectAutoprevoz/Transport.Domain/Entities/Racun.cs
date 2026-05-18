using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_racuni")]
public class Racun
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Required]
    [MaxLength(20)]
    public string BrojRacuna { get; set; } = null!;

    public int? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? PartnerNaziv { get; set; }

    [MaxLength(15)]
    public string? PartnerPIB { get; set; }

    public DateTime DatumDokumenta { get; set; }

    public DateTime? DatumRoka { get; set; }

    [MaxLength(50)]
    public string Valuta { get; set; } = "RSD";

    public decimal Kurs { get; set; } = 1.0m;

    public decimal BrutoIznos { get; set; }

    public decimal PDV { get; set; }

    public decimal Ukupno { get; set; }

    public decimal Placeno { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Neplaceno";

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public int? Brisano { get; set; } = 0;

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<StavkaRacuna> Stavke { get; set; } = [];
}
