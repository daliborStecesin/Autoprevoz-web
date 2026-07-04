using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_KarticaNova")]
public class KarticaNova
{
    [Key]
    public int Id { get; set; }

    public int? IdPartnera { get; set; }

    [MaxLength(20)]
    public string? PIB { get; set; }

    [MaxLength(150)]
    public string? NazivPartnera { get; set; }

    [Required]
    [MaxLength(15)]
    public string PartnerUloga { get; set; } = "";   // KUPAC / DOBAVLJAC

    [Required]
    [MaxLength(25)]
    public string TipDokumenta { get; set; } = "";   // RACUN / UPLATA / KNJIZNO_ODOBRENJE / KNJIZNO_ZADUZENJE / POCETNO

    [MaxLength(50)]
    public string? BrojDokumenta { get; set; }

    public int? IdRacun { get; set; }

    // Veza ka tbl_eFakturaUlaz.idEfakture — NULL za sve ostale tipove unosa
    // (računi bez e-fakture porekla, uplate, isplate, ručni unosi). Sprečava dupli
    // upis pri "Upiši u karticu" akciji sa ulaznih e-faktura.
    public int? IdEfakture { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Duguje { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Potrazuje { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Saldo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Preostalo { get; set; }

    public bool Izmiren { get; set; }

    [Required]
    [MaxLength(3)]
    public string Valuta { get; set; } = "RSD";

    [Column(TypeName = "date")]
    public DateTime? DatumDokumenta { get; set; }

    [Column(TypeName = "date")]
    public DateTime? DatumPrometa { get; set; }

    [Column(TypeName = "date")]
    public DateTime? DatumValute { get; set; }

    public int? IdStavkeVeza { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Kurs { get; set; }

    [MaxLength(50)]
    public string? Izvod { get; set; }

    [MaxLength(255)]
    public string? Opis { get; set; }

    public int? Uneo { get; set; }

    public DateTime? DatumUnosa { get; set; }

    public int? Izmenio { get; set; }

    public DateTime? DatumIzmene { get; set; }
}
