using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_VatDeductionRecord")]
public class VatDeductionRecord
{
    [Key]
    [Column("IdUnosa")]
    public int IdUnosa { get; set; }

    public int RacunBroj { get; set; }

    [MaxLength(20)]
    public string? BrojRacuna { get; set; }

    public DateTime DatumRacuna { get; set; }

    public int? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? PartnerNaziv { get; set; }

    [MaxLength(15)]
    public string? PartnerPIB { get; set; }

    public decimal BrutoIznos { get; set; }

    public decimal IznosPDV { get; set; }

    public decimal IznosUkupno { get; set; }

    [MaxLength(50)]
    public string? TipRacuna { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Neprosledjen";

    [MaxLength(100)]
    public string? RefBroj { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
