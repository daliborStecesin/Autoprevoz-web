using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_partneri")]
public class Partner
{
    [Key]
    [Column("Broj")]
    public int Broj { get; set; }

    [Required]
    [MaxLength(100)]
    public string Naziv { get; set; } = null!;

    [MaxLength(15)]
    public string? PIB { get; set; }

    [MaxLength(100)]
    public string? Mesto { get; set; }

    [MaxLength(100)]
    public string? Adresa { get; set; }

    [MaxLength(20)]
    public string? Telefon { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? BrojRacuna { get; set; }

    [MaxLength(20)]
    public string? BankaBankar { get; set; }

    public int? Brisano { get; set; } = 0;

    public DateTime? DatumUnosa { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<KarticaPartnera> Kartice { get; set; } = [];
    public virtual ICollection<Racun> Racuni { get; set; } = [];
    public virtual ICollection<GotovinskiRacun> GotovinskiRacuni { get; set; } = [];
}
