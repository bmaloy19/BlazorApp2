using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp2.Data.Models;

/// <summary>
/// Category values for service items.
/// </summary>
public static class ServiceCategory
{
    public const string OilChange = "oil_change";
    public const string Tire = "tire";
    public const string Brake = "brake";
    public const string Battery = "battery";
    public const string Fluid = "fluid";
    public const string Filter = "filter";
    public const string Suspension = "suspension";
    public const string Steering = "steering";
    public const string Drivetrain = "drivetrain";
    public const string Transmission = "transmission";
    public const string Engine = "engine";
    public const string Electrical = "electrical";
    public const string AcHeat = "ac_heat";
    public const string Body = "body";
    public const string Inspection = "inspection";
    public const string Recall = "recall";
    public const string Other = "other";

    public static readonly (string Value, string Label)[] All =
    {
        (OilChange, "Oil Change"),
        (Tire, "Tire"),
        (Brake, "Brake"),
        (Battery, "Battery"),
        (Fluid, "Fluid"),
        (Filter, "Filter"),
        (Suspension, "Suspension"),
        (Steering, "Steering"),
        (Drivetrain, "Drivetrain"),
        (Transmission, "Transmission"),
        (Engine, "Engine"),
        (Electrical, "Electrical"),
        (AcHeat, "A/C & Heat"),
        (Body, "Body"),
        (Inspection, "Inspection"),
        (Recall, "Recall"),
        (Other, "Other"),
    };

    public static string GetDisplayName(string category)
    {
        foreach (var (value, label) in All)
        {
            if (value == category) return label;
        }
        return category;
    }
}

/// <summary>
/// Status values for service items.
/// </summary>
public static class ItemStatus
{
    public const string Replaced = "replaced";
    public const string Ok = "ok";
    public const string Attention = "attention";
    public const string Na = "na";

    public static readonly (string Value, string Label)[] All =
    {
        (Replaced, "Replaced"),
        (Ok, "OK / Good"),
        (Attention, "Needs Attention"),
        (Na, "N/A"),
    };

    public static string GetDisplayName(string status) => status switch
    {
        Replaced => "Replaced",
        Ok => "OK",
        Attention => "Needs Attention",
        Na => "N/A",
        _ => status
    };
}

/// <summary>
/// One task/job within a service record.
/// </summary>
[Table("service_items")]
public class ServiceItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("record_id")]
    public long RecordId { get; set; }

    [Required]
    [Column("category")]
    public string Category { get; set; } = ServiceCategory.Other;

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("notes")]
    public string? Notes { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = ItemStatus.Replaced;

    [Column("parts_cost")]
    public decimal? PartsCost { get; set; }

    [Column("labor_cost")]
    public decimal? LaborCost { get; set; }

    [Column("next_due_miles")]
    public int? NextDueMiles { get; set; }

    [Column("next_due_date")]
    public DateOnly? NextDueDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(RecordId))]
    public ServiceRecord Record { get; set; } = null!;
}
