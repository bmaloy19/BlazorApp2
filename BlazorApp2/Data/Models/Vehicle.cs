using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorApp2.Data;

namespace BlazorApp2.Data.Models;

[Table("vehicles")]
public class Vehicle
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [MaxLength(128)]
    [Column("original_owner_user_id")]
    public string? OriginalOwnerUserId { get; set; }

    [Column("make_id")]
    public int? MakeId { get; set; }

    [MaxLength(120)]
    [Column("model")]
    public string? Model { get; set; }

    [Column("year")]
    public int? Year { get; set; }

    [MaxLength(120)]
    [Column("trim")]
    public string? Trim { get; set; }
    
    [MaxLength(60)]
    [Column("body_class")]
    public string? BodyClass { get; set; }

    [MaxLength(40)]
    [Column("vehicle_type")]
    public string? VehicleType { get; set; }

    [MaxLength(80)]
    [Column("series")]
    public string? Series { get; set; }

    [MaxLength(17)]
    [Column("vin")]
    public string? Vin { get; set; }

    [MaxLength(20)]
    [Column("license_plate")]
    public string? LicensePlate { get; set; }

    [MaxLength(60)]
    [Column("color")]
    public string? Color { get; set; }

    [MaxLength(120)]
    [Column("engine")]
    public string? Engine { get; set; }

    [MaxLength(60)]
    [Column("drivetrain")]
    public string? Drivetrain { get; set; }

    [Column("current_miles")]
    public int? CurrentMiles { get; set; }

    [Column("current_hours")]
    public decimal? CurrentHours { get; set; }

    [Column("track_hours")]
    public bool TrackHours { get; set; } = false;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("original_json")]
    public string? OriginalJson { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(MakeId))]
    public Make? Make { get; set; }

    [ForeignKey(nameof(OriginalOwnerUserId))]
    public ApplicationUser? OriginalOwner { get; set; }

    public ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}
