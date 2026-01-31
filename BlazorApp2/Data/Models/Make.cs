using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp2.Data.Models;

[Table("makes")]
public class Make
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("manufacturer_code")]
    public string? ManufacturerCode { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
