using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_partneri")]
public class Partner
{
    [Key]
    public int Broj { get; set; }

    [MaxLength(200)] public string?  Naziv      { get; set; }
    [MaxLength(50)]  public string?  PIB        { get; set; }
    [MaxLength(200)] public string?  Adresa     { get; set; }
    [MaxLength(200)] public string?  Mesto      { get; set; }
    [MaxLength(50)]  public string?  PosBroj    { get; set; }
    [MaxLength(50)]  public string?  Telefon1   { get; set; }
    [MaxLength(50)]  public string?  Telefon2   { get; set; }
    [MaxLength(50)]  public string?  Mobilni    { get; set; }
    [MaxLength(50)]  public string?  Fax        { get; set; }

    [Column("mail")]      [MaxLength(35)]  public string? mail      { get; set; }
    [Column("Racun")]     [MaxLength(50)]  public string? Racun     { get; set; }
    [Column("Banka")]     [MaxLength(50)]  public string? Banka     { get; set; }
    [Column("www")]       [MaxLength(35)]  public string? Www       { get; set; }
    [Column("Kon_osoba")] [MaxLength(35)]  public string? KonOsoba  { get; set; }
    [Column("Sifra")]     [MaxLength(15)]  public string? Sifra     { get; set; }
    [Column("Status")]    [MaxLength(15)]  public string? Status    { get; set; }
    [Column("napomena")]                   public string? Napomena  { get; set; }

    public int? Brisano { get; set; } = 0;

    // Ove kolone ne postoje u tbl_partneri — čuvaju se samo u memoriji
    [NotMapped] public DateTime? DatumUnosa  { get; set; }
    [NotMapped] public DateTime? DatumIzmene { get; set; }

    // Navigacione kolekcije
    public virtual ICollection<KarticaPartnera> Kartice          { get; set; } = [];
    public virtual ICollection<Racun>           Racuni           { get; set; } = [];
    public virtual ICollection<GotovinskiRacun> GotovinskiRacuni { get; set; } = [];
    public virtual ICollection<PartnerRacun>    ZiroRacuni       { get; set; } = [];
}
