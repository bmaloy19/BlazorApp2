using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorApp2.Data;

namespace BlazorApp2.Data.Models;

/// <summary>
/// Record types for service records.
/// </summary>
public static class RecordType
{
    public const string Service = "service";
    public const string Odometer = "odometer";
    public const string Inspection = "inspection";
    public const string Note = "note";

    public static readonly string[] All = { Service, Odometer, Inspection, Note };

    public static string GetDisplayName(string type) => type switch
    {
        Service => "Service",
        Odometer => "Odometer Update",
        Inspection => "Inspection",
        Note => "Note",
        _ => type
    };

    public static string GetIcon(string type) => type switch
    {
        Service => "wrench",
        Odometer => "speedometer",
        Inspection => "clipboard-check",
        Note => "journal-text",
        _ => "question-circle"
    };
}

[Table("service_records")]
public class ServiceRecord
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("vehicle_id")]
    public long VehicleId { get; set; }

    [Required]
    [Column("created_by_user_id")]
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// What kind of record: service | odometer | inspection | note
    /// </summary>
    [Required]
    [Column("record_type")]
    public string RecordType { get; set; } = Models.RecordType.Service;

    [Column("record_date")]
    public DateOnly RecordDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Column("miles_at_service")]
    public int? MilesAtService { get; set; }

    [Column("hours_at_service")]
    public decimal? HoursAtService { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("shop_name")]
    public string? ShopName { get; set; }

    [Column("technician")]
    public string? Technician { get; set; }

    [Column("labor_cost")]
    public decimal? LaborCost { get; set; }

    [Column("parts_cost")]
    public decimal? PartsCost { get; set; }

    /// <summary>
    /// Database-computed column: COALESCE(labor_cost, 0) + COALESCE(parts_cost, 0)
    /// </summary>
    [Column("total_cost")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? TotalCost { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public ICollection<ServiceItem> Items { get; set; } = new List<ServiceItem>();
}
