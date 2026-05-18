using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_Podaci")]
public class PodaciFirme
{
    [Key]
    public int Broj { get; set; }

    [Column("Naziv_Firme")]  [MaxLength(200)] public string? NazivFirme    { get; set; }
    [Column("Maticni_Broj")] [MaxLength(20)]  public string? MaticniBroj   { get; set; }
                             [MaxLength(20)]  public string? PIB            { get; set; }
                             [MaxLength(100)] public string? Mesto          { get; set; }
    [Column("Pos_broj")]     [MaxLength(20)]  public string? PosBroj        { get; set; }
                             [MaxLength(200)] public string? Adresa         { get; set; }
                             [MaxLength(100)] public string? Vlasnik        { get; set; }
    [Column("Sifra_Delatnosti")] [MaxLength(20)] public string? SifraDelatnosti { get; set; }
    [Column("EP_PDV")]       [MaxLength(50)]  public string? EpPdv          { get; set; }
                             [MaxLength(30)]  public string? Telefon        { get; set; }
                             [MaxLength(30)]  public string? Fax            { get; set; }
                             [MaxLength(30)]  public string? mobilni        { get; set; }
                             [MaxLength(30)]  public string? mobilni2       { get; set; }
                             [MaxLength(200)] public string? mail           { get; set; }
                             [MaxLength(200)] public string? sajt           { get; set; }
                             [MaxLength(200)] public string? mailRacuna     { get; set; }
                             [MaxLength(200)] public string? sifraMaila     { get; set; }
    [Column("Ziro_Racun")]   [MaxLength(50)]  public string? ZiroRacun      { get; set; }
                             [MaxLength(100)] public string? Banka          { get; set; }
                             [MaxLength(50)]  public string? inoRacun       { get; set; }
                             [MaxLength(50)]  public string? iban            { get; set; }
                             [MaxLength(20)]  public string? swift           { get; set; }
                             [MaxLength(100)] public string? drzava         { get; set; }
    [MaxLength(500)]                          public string? logoPath        { get; set; }

    // PDV napomene
    [Column("Napomena_PDV")]    public string? NapomenaPDV           { get; set; }
    [Column("Napomena_bezPDV")] public string? NapomenaBezPdvIzvoz   { get; set; }
    [Column("Napomena_inoPDV")] public string? NapomenaBezPdvUvoz    { get; set; }
    [Column("Napomena_1")]      public string? NapomenaInoPdvIzvoz   { get; set; }
    [Column("Napomena_2")]      public string? NapomenaInoPdvUvoz    { get; set; }

    // Trajne napomene na računu
    [Column("Napomena_3")]      public string? NapomenaTrajne1       { get; set; }
    [Column("Napomena_4")]      public string? NapomenaTrajne2       { get; set; }
}
