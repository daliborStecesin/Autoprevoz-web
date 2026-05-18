using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_analitikaEPP")]
public class AnalitikaEpp
{
    [MaxLength(20)]
    public string? Kolona1 { get; set; }

    [MaxLength(50)]
    public string? Kolona2 { get; set; }

    [MaxLength(100)]
    public string? Kolona3 { get; set; }

    [MaxLength(20)]
    public string? Kolona4 { get; set; }

    public decimal? Kolona5 { get; set; }

    public DateTime? Kolona6 { get; set; }

    [MaxLength(500)]
    public string? Napomena { get; set; }
}
