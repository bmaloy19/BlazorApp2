using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorApp2.Data;

namespace BlazorApp2.Data.Models;

[Table("service_records")]
public class ServiceRecord
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("vehicle_id")]
    public long VehicleId { get; set; }

    [Required]
    [MaxLength(128)]
    [Column("created_by_user_id")]
    public string CreatedByUserId { get; set; } = string.Empty;

    [Column("service_date")]
    public DateOnly ServiceDate { get; set; }

    [Column("miles_at_service")]
    public int? MilesAtService { get; set; }

    [Column("hours_at_service")]
    public int? HoursAtService { get; set; }

    [Required]
    [MaxLength(160)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("labor_cost")]
    public decimal LaborCost { get; set; } = 0.00m;

    [Column("parts_cost")]
    public decimal PartsCost { get; set; } = 0.00m;

    [Column("total_cost")]
    public decimal TotalCost { get; set; } = 0.00m;

    [MaxLength(160)]
    [Column("shop_name")]
    public string? ShopName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
