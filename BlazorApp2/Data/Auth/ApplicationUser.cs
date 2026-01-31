using BlazorApp2.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp2.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    // Navigation properties for vehicle maintenance
    public ICollection<Vehicle> OwnedVehicles { get; set; } = new List<Vehicle>();
    public ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}