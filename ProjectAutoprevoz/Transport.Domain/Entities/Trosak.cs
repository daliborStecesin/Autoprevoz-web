using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_troskovi")]
public class Trosak : IAuditable
{
    [Key]
    public int trosakId { get; set; }

    [MaxLength(45)]  public string?  vrstaTroska   { get; set; }
    [MaxLength(45)]  public string?  tipDokumenta  { get; set; }
    [MaxLength(20)]  public string?  idDokumenta   { get; set; }  // varchar(20) u bazi
    public DateTime? datum        { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal?  vrednost     { get; set; }

    [MaxLength(45)]  public string?  tip           { get; set; }
    [MaxLength(500)] public string?  opis          { get; set; }

    public int? racunId    { get; set; }
    public int? idNaloga  { get; set; }
    public int? idDnevnice { get; set; }
    public int? idVozaca  { get; set; }
    public int? idVozila  { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal?  kurs          { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal?  vrednostEUR   { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal?  vrednostEURaut { get; set; }  // AS (kurs*vrednost)

    public int ideTroskovnik { get; set; } = 1;   // int NOT NULL DEFAULT 1
    public int jeGotovinski  { get; set; } = 0;   // int NOT NULL DEFAULT 0
    public int brisano       { get; set; } = 0;   // int NOT NULL DEFAULT 0

    public Vozilo? Vozilo { get; set; }

    public DateTime? DatumUnosa  { get; set; }
    public int?      Izmenio     { get; set; }
    public DateTime? DatumIzmene { get; set; }
}
