using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_banka")]
public class Banka
{
    [Key]
    public int idBanke { get; set; }

    [MaxLength(200)] public string? banka     { get; set; }
    [MaxLength(50)]  public string? racun     { get; set; }
    [MaxLength(20)]  public string? SWIFT     { get; set; }
    [MaxLength(50)]  public string? IBAN      { get; set; }
    [MaxLength(20)]  public string? TipRacuna { get; set; }
                     public int?    aktivan   { get; set; }
                     public int     defaultRacun { get; set; } = 0;
}
