using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_NalogPrevoz")]
public class NalogPrevoz
{
    [Key]
    [Column("idNaloga")]
    public int IdNaloga { get; set; }

    [Column("idPutnogNaloga")]
    public int? IdPutnogNaloga { get; set; }         // FK → tbl_putniNalogKamion.idNaloga

    [Column("brNaloga")]
    [MaxLength(20)]
    public string? BrNaloga { get; set; }            // "20260056" — auto broj

    [Column("tipNaloga")]
    [MaxLength(20)]
    public string? TipNaloga { get; set; }           // SOPSTVENI/AGENCIJSKI

    [Column("idVozila")]
    public int? IdVozila { get; set; }

    [Column("vozilo")]
    [MaxLength(50)]
    public string? Vozilo { get; set; }              // "SO130-MF/BG123 LL" — vozilo+prikolica

    [Column("idPartnera")]
    public int? IdPartnera { get; set; }             // Prevoznik — partner FK

    [Column("datumDokumenta")]
    public DateTime? DatumDokumenta { get; set; }

    [Column("datumUtovara")]
    public DateTime? DatumUtovara { get; set; }

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

    // PAŽNJA: kolona "uvoznik" u DB sadrži ime NALOGODAVCA (legacy naziv!)
    [Column("uvoznik")]
    [MaxLength(200)]
    public string? Uvoznik { get; set; }             // Nalogodavac — naziv partnera

    [Column("idUvoznika")]
    public int? IdUvoznika { get; set; }             // Nalogodavac — partner FK

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

    // PAŽNJA: kolona "napomena2" u DB sadrži ime PREVOZNIKA (legacy naziv!)
    [Column("napomena2")]
    public string? Napomena2 { get; set; }           // Prevoznik — naziv

    [Column("idVozaca")]
    public int? IdVozaca { get; set; }

    [Column("vozac")]
    [MaxLength(50)]
    public string? Vozac { get; set; }

    [Column("cenaTransporta")]
    public decimal? CenaTransporta { get; set; }

    [Column("pdv")]
    public int? Pdv { get; set; }                    // ValutaDana (legacy "pdv" naziv u DB)

    [Column("kursEUR")]
    public decimal? KursEUR { get; set; }

    [Column("placenTransport")]
    public decimal? PlacenTransport { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "U TOKU";

    [Column("brTure")]
    [MaxLength(20)]
    public string? BrTure { get; set; }              // denormalizovano iz parent ture

    [Column("tipRacuna")]
    [MaxLength(15)]
    public string? TipRacuna { get; set; }           // IZLAZ/INOSTRANI/IZLAZ_BP

    [Column("uvoznik2")]
    [MaxLength(200)]
    public string? Uvoznik2 { get; set; }

    [Column("sastavio")]
    [MaxLength(100)]
    public string? Sastavio { get; set; }

    [Column("idDispecer")]
    public int? IdDispecer { get; set; }

    [Column("interniKomentar")]
    [MaxLength(200)]
    public string? InterniKomentar { get; set; }

    [Column("valutaTure")]
    [MaxLength(30)]
    public string? ValutaTure { get; set; }          // "INO (EUR)" / "DOM (RSD)"

    [Column("JM")]
    [MaxLength(15)]
    public string? JM { get; set; }                  // Jedinica mere: KOM/KG/t

    [Column("cenaJM")]
    public decimal? CenaJM { get; set; }

    [Column("kolicina")]
    public decimal? Kolicina { get; set; }

    [Column("VrednostDomaca")]
    public decimal? VrednostDomaca { get; set; }

    [Column("CMR")]
    [MaxLength(100)]
    public string? CMR { get; set; }

    [Column("nalog")]
    [MaxLength(100)]
    public string? Nalog { get; set; }               // referentni broj naloga od klijenta

    [Column("idRacuna")]
    public int? IdRacuna { get; set; }

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

    [Column("idStavkeRacuna")]
    public int? IdStavkeRacuna { get; set; }

    [Column("fakturisanje")]
    public int? Fakturisanje { get; set; }

    [Column("fakturisano")]
    public int? Fakturisano { get; set; }

    [Column("brisano")]
    public int? Brisano { get; set; }

    public virtual PutniNalogKamion? Tura { get; set; }
}
