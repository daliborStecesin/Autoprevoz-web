using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_vozila")]
public class Vozilo
{
    [Key]
    [Column("voziloId")]
    public int VoziloId { get; set; }

    // Identifikacija
    [MaxLength(45)]  public string? brojVozila           { get; set; }
    [MaxLength(45)]  public string? marka                { get; set; }
    [MaxLength(45)]  public string? tip                  { get; set; }
    [MaxLength(45)]  public string? oznaka               { get; set; }
    [MaxLength(30)]  public string? registarskaOznaka    { get; set; }
    [MaxLength(50)]  public string? vrstaVozila          { get; set; }
    [MaxLength(45)]  public string? model                { get; set; }
    [MaxLength(50)]  public string? brSasije             { get; set; }
    [MaxLength(100)] public string? brMotora             { get; set; }
    [MaxLength(100)] public string? Gorivo               { get; set; }

    // snagaKW, zapremina, masa, maxMasa, nosivost su varchar u bazi!
    [MaxLength(20)]  public string? snagaKW              { get; set; }
    [MaxLength(20)]  public string? zapremina            { get; set; }
    [MaxLength(20)]  public string? masa                 { get; set; }
    [MaxLength(20)]  public string? maxMasa              { get; set; }
    [MaxLength(20)]  public string? nosivost             { get; set; }

    // Datumi — Datum_Kupovine/Registracije su datetime u bazi
    [Column("Datum_Kupovine")]            public DateTime? DatumKupovine            { get; set; }
    [Column("Datum_Registracije")]        public DateTime? DatumRegistracije        { get; set; }
    [Column("Datum_SledeceRegistracije")] public DateTime? DatumSledeceRegistracije { get; set; }
    public DateOnly? datumVazenjaRegistracije { get; set; }
    public DateOnly? datumIstekaSD            { get; set; }

    // godinaProizvodnje je date tip u bazi
    public DateOnly? godinaProizvodnje { get; set; }

    // Kilometraža
    [Column("Poc_Kilometraza")] public decimal? PocKilometraza { get; set; }
    public decimal? Kilometraza       { get; set; }
    public decimal? prosekPotrosnje   { get; set; }

    // Vozač
    public int?     vozacId  { get; set; }
    [MaxLength(45)]  public string? Vozac               { get; set; }

    // Ostalo
    [MaxLength(30)]  public string? JedBrVozila         { get; set; }
    [MaxLength(20)]  public string? serBrSaobracajne    { get; set; }
    [MaxLength(50)]  public string? boja                { get; set; }
    [MaxLength(20)]  public string? JMBGvlasnika        { get; set; }
    [MaxLength(100)] public string? PrezimeNazivVlasnika { get; set; }
    [MaxLength(50)]  public string? imeVlasnika         { get; set; }
    [MaxLength(200)] public string? adresaVlasnika      { get; set; }
    public decimal?  vrednostNovog    { get; set; }
    public decimal?  vrednostTrenutno { get; set; }
    [MaxLength(150)] public string? dimenzijeTovarnog   { get; set; }
    [MaxLength(50)]  public string? tipTovarnog         { get; set; }

    public int aktivan { get; set; } = 1;

    // Navigacija
    public virtual ICollection<NalogPrevoz> Nalozi      { get; set; } = [];
    public virtual ICollection<VazniDatum>  VazniDatumi { get; set; } = [];
}
