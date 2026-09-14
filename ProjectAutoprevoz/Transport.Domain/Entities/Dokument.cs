using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_dokumenti")]
public class Dokument : IAuditable
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [MaxLength(15)]
    public string TipDokumenta { get; set; } = null!;

    [MaxLength(15)]
    public string? BrojDokumenta { get; set; }

    [MaxLength(200)]
    public string? Naziv { get; set; }

    [MaxLength(50)]
    public string? PIB { get; set; }

    [Column("Mesto_Izdavanja")]
    [MaxLength(25)]
    public string? MestoIzdavanja { get; set; }

    [Column("Datum_Dokumenta")]
    public DateTime? DatumDokumenta { get; set; }

    [Column("Datum_Vazenosti")]
    public DateTime? DatumVazenosti { get; set; }

    [MaxLength(200)]
    public string? Adresa { get; set; }

    [MaxLength(50)]
    public string? PosBroj { get; set; }

    [MaxLength(200)]
    public string? Mesto { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Osnovica { get; set; }

    [Column("Suma_Rabat", TypeName = "decimal(18,2)")]
    public decimal? SumaRabat { get; set; }

    [Column("Suma_PDV", TypeName = "decimal(18,2)")]
    public decimal? SumaPDV { get; set; }

    [Column("Suma_BezRabata", TypeName = "decimal(18,2)")]
    public decimal? SumaBezRabata { get; set; }

    [Column("Suma_Ukupno", TypeName = "decimal(18,2)")]
    public decimal? SumaUkupno { get; set; }

    [MaxLength(200)]
    public string? Komentar1 { get; set; }

    [MaxLength(200)]
    public string? Komentar2 { get; set; }

    [Column("komentar3")]
    public string? Komentar3 { get; set; }

    [MaxLength(15)]
    public string? Status { get; set; }

    [MaxLength(15)]
    public string? TipProdaje { get; set; }

    [Column("Id_Partnera")]
    public int? IdPartnera { get; set; }

    [Column("idBanke")]
    public int? IdBanke { get; set; }

    [Column("uvozIzvoz")]
    [MaxLength(10)]
    public string? UvozIzvoz { get; set; }

    [Column("kurs", TypeName = "decimal(18,4)")]
    public decimal? Kurs { get; set; }

    [Column("datumKursa")]
    public DateTime? DatumKursa { get; set; }

    [Column("tipStampe")]
    [MaxLength(15)]
    public string? TipStampe { get; set; }

    [Column("idIzvora")]
    public int? IdIzvora { get; set; }

    [Column("tipIzvora")]
    [MaxLength(15)]
    public string? TipIzvora { get; set; }

    /// Ko je IZDAO dokument — upisuje se jednom pri kreiranju (SaveChangesAsync,
    /// isti obrazac kao KarticaPartnera.uneo). Van IAuditable namerno — vidi
    /// komentar u IAuditable.cs.
    public int? uneo { get; set; }

    public DateTime? DatumUnosa  { get; set; }
    public int?      Izmenio     { get; set; }
    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<StavkaDokumenta> Stavke { get; set; } = [];
}
