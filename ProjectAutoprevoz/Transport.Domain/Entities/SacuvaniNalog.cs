using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_sacuvaniNalozi")]
public class SacuvaniNalog
{
    [Key]
    [Column("idNaloga")]
    public int IdNaloga { get; set; }

    [Column("NazivNaloga")]
    [MaxLength(50)]
    public string? NazivNaloga { get; set; }

    [Column("tipNaloga")]
    [MaxLength(20)]
    public string? TipNaloga { get; set; }

    [Column("idPartnera")]
    public int? IdPartnera { get; set; }

    [Column("mestoUtovara")]
    [MaxLength(250)]
    public string? MestoUtovara { get; set; }

    [Column("adresaUtovara")]
    public string? AdresaUtovara { get; set; }

    [Column("podaciORobi")]
    public string? PodaciORobi { get; set; }

    [Column("kontakt")]
    [MaxLength(100)]
    public string? Kontakt { get; set; }

    [Column("izvoznoCarinjenje")]
    [MaxLength(500)]
    public string? IzvoznoCarinjenje { get; set; }

    [Column("uvoznik")]
    [MaxLength(200)]
    public string? Uvoznik { get; set; }

    [Column("idUvoznika")]
    public int? IdUvoznika { get; set; }

    [Column("uvoznoCarinjenje")]
    [MaxLength(500)]
    public string? UvoznoCarinjenje { get; set; }

    [Column("mestoIstovara")]
    [MaxLength(250)]
    public string? MestoIstovara { get; set; }

    [Column("adresaIstovara")]
    public string? AdresaIstovara { get; set; }

    [Column("datumIstovara")]
    public DateTime? DatumIstovara { get; set; }

    [Column("usloviPlacanja")]
    [MaxLength(200)]
    public string? UsloviPlacanja { get; set; }

    [Column("napomena1")]
    public string? Napomena1 { get; set; }

    [Column("napomena2")]
    public string? Napomena2 { get; set; }

    [Column("cenaTransporta")]
    public decimal? CenaTransporta { get; set; }

    [Column("placenTransport")]
    public decimal? PlacenTransport { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string? Status { get; set; }

    [Column("brTure")]
    [MaxLength(20)]
    public string? BrTure { get; set; }

    [Column("tipRacuna")]
    [MaxLength(15)]
    public string? TipRacuna { get; set; }

    [Column("uvoznik2")]
    [MaxLength(200)]
    public string? Uvoznik2 { get; set; }

    [Column("sastavio")]
    [MaxLength(100)]
    public string? Sastavio { get; set; }

    [Column("valutaTure")]
    [MaxLength(30)]
    public string? ValutaTure { get; set; }

    [Column("JM")]
    [MaxLength(15)]
    public string? JM { get; set; }

    [Column("cenaJM")]
    public decimal? CenaJM { get; set; }

    [Column("kolicina")]
    public decimal? Kolicina { get; set; }

    [Column("VrednostDomaca")]
    public decimal? VrednostDomaca { get; set; }

    [Column("nalog")]
    [MaxLength(100)]
    public string? Nalog { get; set; }

    [Column("sifraTransporta")]
    [MaxLength(50)]
    public string? SifraTransporta { get; set; }

    [Column("datumKursa")]
    public DateTime? DatumKursa { get; set; }

    [Column("OpisSifre")]
    [MaxLength(200)]
    public string? OpisSifre { get; set; }

    [Column("placenTransportDin")]
    public decimal? PlacenTransportDin { get; set; }

    [Column("troskoviDin")]
    public decimal? TroskoviDin { get; set; }

    [Column("zaradaDin")]
    public decimal? ZaradaDin { get; set; }
}
