using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_putniNalogKamion")]
public class PutniNalogKamion
{
    [Key]
    [Column("idNaloga")]
    public int IdNaloga { get; set; }

    [Column("brNaloga")]
    [MaxLength(50)]
    public string? BrNaloga { get; set; }           // BrTure: "119/26"

    [Column("tipPrevoza")]
    [MaxLength(50)]
    public string? TipPrevoza { get; set; }          // ZEMLJA/INOSTRANSTVO/KOMBINOVANO

    [Column("vrstaPrevoza")]
    [MaxLength(50)]
    public string VrstaPrevoza { get; set; } = "SOPSTVENI";  // SOPSTVENI/AGENCIJSKI

    [Column("Cilj")]
    [MaxLength(50)]
    public string? Cilj { get; set; }

    [Column("RadnoMesto")]
    [MaxLength(50)]
    public string? RadnoMesto { get; set; }          // "Utovario" u WinForms UI

    [Column("vozac1")]
    [MaxLength(50)]
    public string? Vozac1 { get; set; }

    [Column("vozac2")]
    [MaxLength(50)]
    public string? Vozac2 { get; set; }

    [Column("idVozaca1")]
    public int? IdVozaca1 { get; set; }

    [Column("idVozaca2")]
    public int? IdVozaca2 { get; set; }

    [Column("planiranoDana")]
    [MaxLength(5)]
    public string? PlaniranoDana { get; set; }

    [Column("AkontacijaDom")]
    public decimal? AkontacijaDom { get; set; }

    [Column("AkontacijaIno")]
    public decimal? AkontacijaIno { get; set; }

    [Column("relacija")]
    [MaxLength(500)]
    public string? Relacija { get; set; }

    [Column("datumPolaska")]
    public DateTime? DatumPolaska { get; set; }

    [Column("datumIzlaska")]
    public DateTime? DatumIzlaska { get; set; }      // prelazak granice — odlazak

    [Column("datumUlaska")]
    public DateTime? DatumUlaska { get; set; }       // prelazak granice — povratak

    [Column("datumDolaska")]
    public DateTime? DatumDolaska { get; set; }

    [Column("idVozila")]
    public int? IdVozila { get; set; }

    [Column("vozilo")]
    [MaxLength(50)]
    public string? Vozilo { get; set; }              // registarska oznaka kamiona

    [Column("satiNaPutuUK")]
    public decimal? SatiNaPutuUK { get; set; }

    [Column("satiNaPutuDom")]
    public decimal? SatiNaPutuDom { get; set; }

    [Column("satiNaPutuIno")]
    public decimal? SatiNaPutuIno { get; set; }

    [Column("cenaDnevnicaDom")]
    public decimal? CenaDnevnicaDom { get; set; }    // vrednost jedne domaće dnevnice

    [Column("cenaDnevnicaIno")]
    public decimal? CenaDnevnicaIno { get; set; }    // vrednost jedne ino dnevnice (EUR)

    [Column("satiDom")]
    public decimal? SatiDom { get; set; }

    [Column("dnevnicaDom")]
    public decimal? DnevnicaDom { get; set; }        // broj domaćih dnevnica

    [Column("satiIno")]
    public decimal? SatiIno { get; set; }

    [Column("dnevnicaIno")]
    public decimal? DnevnicaIno { get; set; }        // broj inostranih dnevnica

    [Column("VrednostDnevnicaDom")]
    public decimal? VrednostDnevnicaDom { get; set; }

    [Column("VrednostDnevnicaIno")]
    public decimal? VrednostDnevnicaIno { get; set; }

    [Column("VrednostTroskovaDom")]
    public decimal? VrednostTroskovaDom { get; set; }

    [Column("VrednostTroskovaIno")]
    public decimal? VrednostTroskovaIno { get; set; }

    [Column("PovracajDom")]
    public decimal? PovracajDom { get; set; }

    [Column("PovracajIno")]
    public decimal? PovracajIno { get; set; }

    [Column("idPartnera")]
    public int? IdPartnera { get; set; }

    [Column("partner")]
    [MaxLength(100)]
    public string? Partner { get; set; }

    [Column("kmPocetna")]
    public decimal? KmPocetna { get; set; }

    [Column("kmTrenutna")]
    public decimal? KmTrenutna { get; set; }

    [Column("predjeno")]
    public decimal? Predjeno { get; set; }

    [Column("cena")]
    public decimal? Cena { get; set; }

    [Column("PDVstopa")]
    public decimal? PDVstopa { get; set; }

    [Column("kolicina")]
    public decimal? Kolicina { get; set; }

    [Column("vrednost")]
    public decimal? Vrednost { get; set; }

    [Column("kursEUR")]
    public decimal? KursEUR { get; set; }

    [Column("datumDokumenta")]
    public DateTime? DatumDokumenta { get; set; }

    [Column("idRacuna")]
    public int? IdRacuna { get; set; }

    // vrednostAUTO i vrednostKM su computed kolone u SQL — ne mapirati

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "U TOKU";

    [Column("sastavio")]
    [MaxLength(50)]
    public string? Sastavio { get; set; }

    [Column("idSastavio")]
    public int? IdSastavio { get; set; }

    [Column("idPrikolice")]
    public int? IdPrikolice { get; set; }

    [Column("prikolica")]
    [MaxLength(30)]
    public string? Prikolica { get; set; }

    [Column("brisano")]
    public int? Brisano { get; set; }

    [Column("gorivoStart")]
    public decimal? GorivoStart { get; set; }

    [Column("gorivoEnd")]
    public decimal? GorivoEnd { get; set; }

    [Column("gorivoUtroseno")]
    public decimal? GorivoUtroseno { get; set; }

    [Column("Potrosnja")]
    public decimal? Potrosnja { get; set; }

    public virtual ICollection<NalogPrevoz> Nalozi { get; set; } = [];
}
