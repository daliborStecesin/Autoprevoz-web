using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_dozvole")]
public class VazniDatum
{
    [Key]
    public int idDozvole { get; set; }

    public int? idVozila { get; set; }

    // zemlja = tip dokumenta (REGISTRACIJA, BELA POTVRDA, VINJETA...)
    [Column("zemlja")]
    [MaxLength(50)]
    public string? TipDokumenta { get; set; }

    public DateOnly? datumIzrade { get; set; }
    public DateOnly? datumIsteka { get; set; }

    [MaxLength(20)]
    public string? brDokumenta { get; set; }

    [ForeignKey(nameof(idVozila))]
    public virtual Vozilo? Vozilo { get; set; }
}
