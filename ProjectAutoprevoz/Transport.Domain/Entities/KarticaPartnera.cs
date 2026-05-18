using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_Kartica")]
public class KarticaPartnera
{
    [Key]
    [Column("Id_Kartice")]
    public int Id { get; set; }

    [Column("Id_Partnera")]
    [MaxLength(15)]
    public string? PartnerBroj { get; set; }

    [MaxLength(100)]
    public string? Naziv { get; set; }

    [MaxLength(15)]
    public string? PIB { get; set; }

    public decimal DugoveNam { get; set; }

    public decimal DaoNam { get; set; }

    public decimal Saldo { get; set; }

    public DateTime? DatumUnosa { get; set; }
}
