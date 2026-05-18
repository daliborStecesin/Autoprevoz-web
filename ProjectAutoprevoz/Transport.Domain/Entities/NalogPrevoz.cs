using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_NalogPrevoz")]
public class NalogPrevoz
{
    [Key]
    [Column("IdNaloga")]
    public int IdNaloga { get; set; }

    [MaxLength(20)]
    public string? BrNaloga { get; set; }

    public int? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? PartnerNaziv { get; set; }

    public int? VoziloId { get; set; }

    [MaxLength(50)]
    public string? RegistrskaBroja { get; set; }

    public int? VozacId { get; set; }

    [MaxLength(100)]
    public string? VozacIme { get; set; }

    [MaxLength(50)]
    public string? TipNaloga { get; set; }

    [MaxLength(200)]
    public string? PolaznaLokacija { get; set; }

    [MaxLength(200)]
    public string? OdredisnaLokacija { get; set; }

    public decimal? PredjenoPutnje { get; set; }

    public decimal? Tezina { get; set; }

    [MaxLength(100)]
    public string? VrstaTereta { get; set; }

    public DateTime DatumDokumenta { get; set; }

    public DateTime? DatumIzlaska { get; set; }

    public DateTime? DatumUlaska { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Otvoren";

    public decimal IznosUgovor { get; set; }

    public decimal IznosPDV { get; set; }

    public decimal IznosUkupno { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public int? Brisano { get; set; } = 0;

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<PutniNalogKamion> PutniNalozi { get; set; } = [];
}
