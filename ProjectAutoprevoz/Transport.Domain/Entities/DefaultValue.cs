using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transport.Domain.Entities;

[Table("tbl_DefaultValues")]
public class DefaultValue
{
    [Key]
    public int DefaultId { get; set; }

    [Required, MaxLength(100)]
    public string FormName { get; set; } = "";

    [Required, MaxLength(150)]
    public string ControlName { get; set; } = "";

    [MaxLength(50)]
    public string? ControlType { get; set; }

    [MaxLength(500)]
    public string? ValueText { get; set; }

    public int UserId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
