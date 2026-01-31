using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp2.Data.Models;

[Table("user_vehicles")]
public class UserVehicle
{
    [Required]
    [MaxLength(128)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("vehicle_id")]
    public long VehicleId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = "owner";

    [MaxLength(120)]
    [Column("nickname")]
    public string? Nickname { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;
}
