using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_DozvoleMinistarstva")]
public class DozvolaMinistarstva
{
    [Key] public int idDozvole { get; set; }

    [MaxLength(50)]  public string?  brojResenja           { get; set; }
    public DateOnly? datumResenja         { get; set; }
    [MaxLength(100)] public string?  drzava                { get; set; }
    [MaxLength(50)]  public string?  vrstaDozvole          { get; set; }
    [MaxLength(10)]  public string?  rbVrste               { get; set; }
    [MaxLength(150)] public string?  brojDozvole           { get; set; }
    public DateOnly? datumIsteka          { get; set; }
    public DateOnly? datumIzdavanjaVozacu { get; set; }
    public DateOnly? datumVracanjaUKanc   { get; set; }
    public int?      idVozila             { get; set; }
    [MaxLength(50)]  public string?  vozilo                { get; set; }
    public int?      idVozaca             { get; set; }
    [MaxLength(100)] public string?  Vozac                 { get; set; }
    [MaxLength(50)]  public string?  brojNaloga            { get; set; }
    public int?      idNaloga             { get; set; }
    [MaxLength(500)] public string?  Relacija              { get; set; }
    [MaxLength(10)]  public string?  razduzeno             { get; set; }
    [MaxLength(10)]  public string?  stampa                { get; set; }
    [MaxLength(50)]  public string?  cmr                   { get; set; }
    public DateOnly? datumSlanjaNaRazd    { get; set; }
    public DateOnly? datumRazduzenja      { get; set; }
    public DateOnly? datumDobijanjaResenje { get; set; }
    [MaxLength(10)]  public string?  vracenoUkanc          { get; set; }
    [MaxLength(10)]  public string?  poslatoNaRazd         { get; set; }
    [MaxLength(10)]  public string?  izdataVozacu          { get; set; }
    [MaxLength(100)] public string?  uneo                  { get; set; }
    public DateTime? datumUnosa           { get; set; }
    public int       aktivan              { get; set; } = 1;
}
