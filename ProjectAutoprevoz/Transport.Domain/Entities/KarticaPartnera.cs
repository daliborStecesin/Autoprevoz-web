using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_Kartica")]
public class KarticaPartnera : IAuditable
{
    [Key]
    [Column("Id_Kartice")]
    public int Id { get; set; }

    [Column("Id_Racuna")]
    [MaxLength(15)]
    public string? IdRacuna { get; set; }

    [Column("Id_Partnera")]
    [MaxLength(15)]
    public string? IdPartnera { get; set; }

    [Column("Broj_Racuna")]
    [MaxLength(50)]
    public string? BrojRacuna { get; set; }

    [Column("Naziv_Partnera")]
    [MaxLength(200)]
    public string? NazivPartnera { get; set; }

    [Column("PIB_Partnera")]
    [MaxLength(50)]
    public string? PibPartnera { get; set; }

    [Column("Datum_Racuna")]
    public DateTime? DatumRacuna { get; set; }

    [Column("Datum_Valute")]
    public DateTime? DatumValute { get; set; }

    public decimal? Dug { get; set; }

    public decimal? Uplata { get; set; }

    [Column("Preostalo")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? Preostalo { get; set; }

    [Column("Korisnik_Id")]
    [MaxLength(3)]
    public string? KorisnikId { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; }

    [MaxLength(15)]
    public string? selektor { get; set; }

    [MaxLength(2)]
    public string? Izmiren { get; set; }

    [MaxLength(1)]
    public string? Objekat { get; set; }

    public decimal? Duguje { get; set; }

    public decimal? Potrazuje { get; set; }

    public decimal? Saldo { get; set; }

    [Column("Id_uplate")]
    [MaxLength(15)]
    public string? IdUplate { get; set; }

    [MaxLength(15)]
    public string? izvod { get; set; }

    [MaxLength(500)]
    public string? opis { get; set; }

    public int brisano { get; set; }

    [Column("Datum_Prometa")]
    public DateTime? DatumPrometa { get; set; }

    public int? uneo { get; set; }

    [Column("datumUnosa")]
    public DateTime? DatumUnosa { get; set; }

    [Column("izmenio")]
    public int? Izmenio { get; set; }

    [Column("datumIzmene")]
    public DateTime? DatumIzmene { get; set; }
}
