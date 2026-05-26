using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_imenik")]
public class Vozac
{
    [Key]
    public int Broj { get; set; }

    [MaxLength(50)]
    public string? Ime { get; set; }

    [MaxLength(50)]
    public string? Prezime { get; set; }

    [MaxLength(200)]
    public string? Adresa { get; set; }

    [MaxLength(100)]
    public string? Mesto { get; set; }

    [MaxLength(30)]
    public string? Telefon { get; set; }

    [MaxLength(30)]
    public string? TelefonMob { get; set; }

    [Column("Broj_LK")]
    [MaxLength(30)]
    public string? BrojLK { get; set; }

    [MaxLength(20)]
    public string? JMBG { get; set; }

    [MaxLength(100)]
    public string? Zaposlenje { get; set; }

    [MaxLength(200)]
    public string? Mail { get; set; }

    [MaxLength(500)]
    public string? Komentar { get; set; }

    public DateOnly? istekDozvole { get; set; }

    [MaxLength(50)]
    public string? pasos { get; set; }

    public DateOnly? istekPasosa { get; set; }

    public DateOnly? datumZaposlenja { get; set; }

    public DateOnly? datumRodjenja { get; set; }

    public decimal? dnevnica { get; set; }

    [MaxLength(20)]
    public string? tipIsplate { get; set; }

    public decimal? procenatZaPlatu { get; set; }

    public decimal? fixnoPlata { get; set; }

    public decimal? cenaPoKm { get; set; }

    public int? aktivan { get; set; }
}
