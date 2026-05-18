using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_sacuvaniNalozi")]
public class SacuvaniNalog
{
    [Key]
    [Column("IdNaloga")]
    public int IdNaloga { get; set; }

    [MaxLength(20)]
    public string? BrNaloga { get; set; }

    [MaxLength(100)]
    public string? Naziv { get; set; }

    public int? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? PartnerNaziv { get; set; }

    public int? VoziloId { get; set; }

    [MaxLength(50)]
    public string? RegistrskaBroja { get; set; }

    [MaxLength(200)]
    public string? PolaznaLokacija { get; set; }

    [MaxLength(200)]
    public string? OdredisnaLokacija { get; set; }

    [MaxLength(100)]
    public string? VrstaTereta { get; set; }

    public decimal? PredjenoPutnje { get; set; }

    public decimal? Tezina { get; set; }

    public decimal IznosUgovor { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
