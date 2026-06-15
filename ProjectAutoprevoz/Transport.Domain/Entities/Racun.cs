using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_racuni")]
public class Racun : IAuditable
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Column("Broj_Racuna")]
    [MaxLength(15)]
    public string? BrojRacuna { get; set; }

    [MaxLength(200)]
    public string? Naziv { get; set; }

    [MaxLength(50)]
    public string? PIB { get; set; }

    [Column("Mesto_Izdavanja")]
    [MaxLength(25)]
    public string? MestoIzdavanja { get; set; }

    [Column("Datum_Racuna")]
    public DateTime? DatumRacuna { get; set; }

    [Column("Datum_valute")]
    public DateTime? DatumValute { get; set; }

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

    [Column("Suma_RacunaBezRabata", TypeName = "decimal(18,2)")]
    public decimal? SumaRacunaBezRabata { get; set; }

    [Column("Suma_Racuna", TypeName = "decimal(18,2)")]
    public decimal? SumaRacuna { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [MaxLength(200)]
    public string? Slovima { get; set; }

    [MaxLength(200)]
    public string? Komentar1 { get; set; }

    [MaxLength(200)]
    public string? Komentar2 { get; set; }

    [Column("Fisk_Isecak")]
    [MaxLength(100)]
    public string? FiskIsecak { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [Column("Radni_Nalog")]
    [MaxLength(100)]
    public string? RadniNalog { get; set; }

    [MaxLength(100)]
    public string? Otpremnica { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [MaxLength(15)]
    public string? Selektor { get; set; }

    [MaxLength(15)]
    public string? Status { get; set; }

    [Column("Tip_Prodaje")]
    [MaxLength(15)]
    public string? TipProdaje { get; set; }

    [Column("Id_Partnera")]
    public int? IdPartnera { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [MaxLength(15)]
    public string? Bon { get; set; }

    [MaxLength(50)]
    public string? Vozac { get; set; }

    [MaxLength(50)]
    public string? Vozilo { get; set; }

    [Column("regOznaka")]
    [MaxLength(50)]
    public string? RegOznaka { get; set; }

    [Column("idVozila")]
    [MaxLength(5)]
    public string? IdVozila { get; set; }

    [Column("idVozaca")]
    [MaxLength(5)]
    public string? IdVozaca { get; set; }

    [Column("idBanke")]
    public int? IdBanke { get; set; }

    [Column("Datum_Prometa")]
    public DateTime? DatumPrometa { get; set; }

    [Column("Datum_PrometaDo")]
    public DateTime? DatumPrometaDo { get; set; }

    [Column("komentar3")]
    public string? Komentar3 { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [Column("komentar4")]
    public string? Komentar4 { get; set; }

    [Column("idTure")]
    public int? IdTure { get; set; }

    [Column("idNaloga")]
    public int? IdNaloga { get; set; }

    [Column("idFakturisao")]
    public int? IdFakturisao { get; set; }

    [Column("fakturisao")]
    [MaxLength(70)]
    public string? Fakturisao { get; set; }

    // LEGACY — mapirano radi kompatibilnosti, NE koristiti u UI/logici
    [Column("Korisnik_Id")]
    public int? KorisnikId { get; set; }

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

    public int brisano { get; set; } = 0;

    public DateTime? DatumUnosa  { get; set; }
    public int?      Izmenio     { get; set; }
    public DateTime? DatumIzmene { get; set; }
}
