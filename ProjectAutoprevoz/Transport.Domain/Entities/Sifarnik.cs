using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_sifarnik")]
public class Sifarnik
{
    [Key]
    public int idStavke { get; set; }

    [MaxLength(50)]  public string? kategorija { get; set; }
    [MaxLength(100)] public string? naziv      { get; set; }
    public int? aktivan  { get; set; }
    public int? redosled { get; set; }
}
