namespace BlazorApp2.Data.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public DateTime DatePerformed { get; set; }
    public string Description { get; set; }
    public decimal Cost { get; set; }
    public List<Part> Parts { get; set; } = new();
    public string ReceiptImagePath { get; set; }
}