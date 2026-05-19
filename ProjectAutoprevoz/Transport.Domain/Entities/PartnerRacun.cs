using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_partner_racuni")]
public class PartnerRacun
{
    [Key]
    public int idRacuna { get; set; }

    public int idPartnera { get; set; }

    [MaxLength(50)] public string? ziroRacun   { get; set; }
    [MaxLength(100)] public string? banka      { get; set; }
    [MaxLength(50)] public string? IBAN        { get; set; }

    public int? glavniRacun { get; set; }
    public int? aktivan     { get; set; }

    [ForeignKey(nameof(idPartnera))]
    public virtual Partner? Partner { get; set; }
}
