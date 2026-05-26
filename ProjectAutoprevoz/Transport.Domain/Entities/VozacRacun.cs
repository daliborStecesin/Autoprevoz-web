using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_vozac_racuni")]
public class VozacRacun
{
    [Key]
    public int idRacuna { get; set; }

    public int idVozaca { get; set; }

    [MaxLength(150)]
    public string? nazivBanke { get; set; }

    [MaxLength(50)]
    public string? racun { get; set; }

    [MaxLength(50)]
    public string? IBAN { get; set; }

    [MaxLength(10)]
    public string valuta { get; set; } = "RSD";

    [MaxLength(20)]
    public string tipRacuna { get; set; } = "DOMACI";

    public int glavniRacun { get; set; }

    public int aktivan { get; set; } = 1;

    public Vozac? Vozac { get; set; }
}
