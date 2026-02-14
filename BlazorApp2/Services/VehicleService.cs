using BlazorApp2.Data;
using BlazorApp2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class VehicleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public VehicleService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Get all vehicles for a user (including shared vehicles)
    /// </summary>
    public async Task<List<UserVehicle>> GetUserVehiclesAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserVehicles
            .Include(uv => uv.Vehicle)
                .ThenInclude(v => v.Make)
            .Where(uv => uv.UserId == userId)
            .OrderByDescending(uv => uv.IsPrimary)
            .ThenByDescending(uv => uv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get vehicles owned by the user (Role = "owner")
    /// </summary>
    public async Task<List<UserVehicle>> GetOwnedVehiclesAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserVehicles
            .Include(uv => uv.Vehicle)
                .ThenInclude(v => v.Make)
            .Where(uv => uv.UserId == userId && uv.Role == "owner")
            .OrderByDescending(uv => uv.IsPrimary)
            .ThenByDescending(uv => uv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get vehicles shared with the user (Role != "owner")
    /// </summary>
    public async Task<List<UserVehicle>> GetSharedVehiclesAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserVehicles
            .Include(uv => uv.Vehicle)
                .ThenInclude(v => v.Make)
            .Include(uv => uv.Vehicle)
                .ThenInclude(v => v.UserVehicles)
                    .ThenInclude(uv => uv.User)
            .Where(uv => uv.UserId == userId && uv.Role != "owner")
            .OrderByDescending(uv => uv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get the owner of a vehicle
    /// </summary>
    public async Task<ApplicationUser?> GetVehicleOwnerAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ownerLink = await context.UserVehicles
            .Include(uv => uv.User)
            .FirstOrDefaultAsync(uv => uv.VehicleId == vehicleId && uv.Role == "owner");
        
        return ownerLink?.User;
    }

    /// <summary>
    /// Get a specific vehicle by ID
    /// </summary>
    public async Task<Vehicle?> GetVehicleByIdAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Vehicles
            .Include(v => v.Make)
            .Include(v => v.UserVehicles)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
    }

    /// <summary>
    /// Get a user's primary vehicle
    /// </summary>
    public async Task<UserVehicle?> GetPrimaryVehicleAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserVehicles
            .Include(uv => uv.Vehicle)
                .ThenInclude(v => v.Make)
            .Where(uv => uv.UserId == userId && uv.IsPrimary)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Add a new vehicle and link it to the user as owner
    /// </summary>
    public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle, string userId, string? nickname = null, string? makeName = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Check if THIS user already has a vehicle with this VIN
        if (!string.IsNullOrEmpty(vehicle.Vin))
        {
            var userAlreadyHasVin = await context.UserVehicles
                .Include(uv => uv.Vehicle)
                .AnyAsync(uv => uv.UserId == userId && uv.Vehicle.Vin == vehicle.Vin);
            
            if (userAlreadyHasVin)
            {
                throw new InvalidOperationException("You already have a vehicle with this VIN in your garage.");
            }
        }
        
        // Check if user has any vehicles to determine if this should be primary
        var hasVehicles = await context.UserVehicles.AnyAsync(uv => uv.UserId == userId);
        
        // Handle MakeId - verify it exists or create the make
        if (vehicle.MakeId.HasValue && vehicle.MakeId.Value > 0)
        {
            var makeExists = await context.Makes.AnyAsync(m => m.Id == vehicle.MakeId.Value);
            if (!makeExists)
            {
                // Make doesn't exist - try to create it if we have the name
                if (!string.IsNullOrEmpty(makeName))
                {
                    // Check if make exists by name
                    var existingMake = await context.Makes.FirstOrDefaultAsync(m => m.Name == makeName);
                    if (existingMake != null)
                    {
                        vehicle.MakeId = existingMake.Id;
                    }
                    else
                    {
                        // Create new make
                        var newMake = new Make
                        {
                            Name = makeName,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.Makes.Add(newMake);
                        await context.SaveChangesAsync();
                        vehicle.MakeId = newMake.Id;
                    }
                }
                else
                {
                    // No make name provided, set to null
                    vehicle.MakeId = null;
                }
            }
        }
        else if (!string.IsNullOrEmpty(makeName))
        {
            // MakeId is 0 or null but we have a name - look up or create
            var existingMake = await context.Makes.FirstOrDefaultAsync(m => m.Name == makeName);
            if (existingMake != null)
            {
                vehicle.MakeId = existingMake.Id;
            }
            else
            {
                var newMake = new Make
                {
                    Name = makeName,
                    CreatedAt = DateTime.UtcNow
                };
                context.Makes.Add(newMake);
                await context.SaveChangesAsync();
                vehicle.MakeId = newMake.Id;
            }
        }
        else
        {
            // No valid MakeId and no make name
            vehicle.MakeId = null;
        }
        
        // Don't set Id - let the database generate it (bigint with IDENTITY)
        vehicle.OriginalOwnerUserId = userId;
        vehicle.CreatedAt = DateTime.UtcNow;

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync(); // Save to get the generated Id
        
        // Create the user-vehicle relationship
        context.UserVehicles.Add(new UserVehicle
        {
            UserId = userId,
            VehicleId = vehicle.Id, // Now has the database-generated Id
            Role = "owner",
            Nickname = nickname,
            IsPrimary = !hasVehicles, // First vehicle is primary
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return vehicle;
    }

    /// <summary>
    /// Update an existing vehicle
    /// </summary>
    public async Task<Vehicle?> UpdateVehicleAsync(Vehicle vehicle)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var existing = await context.Vehicles.FindAsync(vehicle.Id);
        if (existing == null)
            return null;

        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;
        existing.Trim = vehicle.Trim;
        existing.Vin = vehicle.Vin;
        existing.LicensePlate = vehicle.LicensePlate;
        existing.Color = vehicle.Color;
        existing.Engine = vehicle.Engine;
        existing.Drivetrain = vehicle.Drivetrain;
        existing.CurrentMiles = vehicle.CurrentMiles;
        existing.CurrentHours = vehicle.CurrentHours;
        existing.TrackHours = vehicle.TrackHours;
        existing.Notes = vehicle.Notes;
        existing.MakeId = vehicle.MakeId;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }


    /// <summary>
    /// Update the user-vehicle relationship (nickname, primary status)
    /// </summary>
    public async Task<bool> UpdateUserVehicleAsync(string userId, long vehicleId, string? nickname = null, bool? isPrimary = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var userVehicle = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VehicleId == vehicleId);
        
        if (userVehicle == null)
            return false;

        if (nickname != null)
            userVehicle.Nickname = nickname;

        if (isPrimary == true)
        {
            // Clear primary from other vehicles
            var otherVehicles = await context.UserVehicles
                .Where(uv => uv.UserId == userId && uv.VehicleId != vehicleId && uv.IsPrimary)
                .ToListAsync();
            
            foreach (var other in otherVehicles)
                other.IsPrimary = false;

            userVehicle.IsPrimary = true;
        }
        else if (isPrimary == false)
        {
            userVehicle.IsPrimary = false;
        }

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Delete a vehicle (only owner can delete)
    /// </summary>
    public async Task<bool> DeleteVehicleAsync(long vehicleId, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var vehicle = await context.Vehicles
            .Include(v => v.UserVehicles)
            .Include(v => v.ServiceRecords)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
        
        if (vehicle == null)
            return false;

        // Check if user is the original owner OR has "owner" role in UserVehicles
        var isOriginalOwner = vehicle.OriginalOwnerUserId == userId;
        var userVehicle = vehicle.UserVehicles.FirstOrDefault(uv => uv.UserId == userId);
        var isOwnerRole = userVehicle?.Role == "owner";
        
        // Allow delete if user is original owner OR has owner role
        if (!isOriginalOwner && !isOwnerRole)
            return false;

        // Remove all service records first (cascade should handle this, but being explicit)
        if (vehicle.ServiceRecords.Any())
        {
            context.ServiceRecords.RemoveRange(vehicle.ServiceRecords);
        }
        
        // Remove all user-vehicle relationships
        if (vehicle.UserVehicles.Any())
        {
            context.UserVehicles.RemoveRange(vehicle.UserVehicles);
        }

        context.Vehicles.Remove(vehicle);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Remove user's access to a vehicle (unlink without deleting)
    /// </summary>
    public async Task<bool> RemoveUserVehicleAsync(string userId, long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var userVehicle = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VehicleId == vehicleId);
        
        if (userVehicle == null)
            return false;

        context.UserVehicles.Remove(userVehicle);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Share a vehicle with another user
    /// </summary>
    public async Task<bool> ShareVehicleAsync(long vehicleId, string ownerUserId, string targetUserId, string role = "viewer")
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Verify the owner has permission to share
        var ownerLink = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == ownerUserId && uv.VehicleId == vehicleId && uv.Role == "owner");
        
        if (ownerLink == null)
            return false;

        // Check if already shared
        var existingShare = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == targetUserId && uv.VehicleId == vehicleId);
        
        if (existingShare != null)
            return false;

        context.UserVehicles.Add(new UserVehicle
        {
            UserId = targetUserId,
            VehicleId = vehicleId,
            Role = role,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Find a user by email address
    /// </summary>
    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
    }

    /// <summary>
    /// Share a vehicle with another user by email
    /// Returns a result with success status and error message if applicable
    /// </summary>
    public async Task<ShareVehicleResult> ShareVehicleByEmailAsync(long vehicleId, string ownerUserId, string targetEmail)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Find target user by email
        var targetUser = await context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == targetEmail.ToUpperInvariant());
        
        if (targetUser == null)
        {
            return new ShareVehicleResult 
            { 
                Success = false, 
                ErrorMessage = "No user found with that email address." 
            };
        }

        // Can't share with yourself
        if (targetUser.Id == ownerUserId)
        {
            return new ShareVehicleResult 
            { 
                Success = false, 
                ErrorMessage = "You cannot share a vehicle with yourself." 
            };
        }

        // Verify the owner has permission to share
        var ownerLink = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == ownerUserId && uv.VehicleId == vehicleId && uv.Role == "owner");
        
        if (ownerLink == null)
        {
            return new ShareVehicleResult 
            { 
                Success = false, 
                ErrorMessage = "You don't have permission to share this vehicle." 
            };
        }

        // Check if already shared with this user
        var existingShare = await context.UserVehicles
            .FirstOrDefaultAsync(uv => uv.UserId == targetUser.Id && uv.VehicleId == vehicleId);
        
        if (existingShare != null)
        {
            return new ShareVehicleResult 
            { 
                Success = false, 
                ErrorMessage = "This vehicle is already shared with that user." 
            };
        }

        // Create the share
        context.UserVehicles.Add(new UserVehicle
        {
            UserId = targetUser.Id,
            VehicleId = vehicleId,
            Role = "viewer",
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        
        return new ShareVehicleResult 
        { 
            Success = true, 
            SharedWithUserEmail = targetUser.Email 
        };
    }
}

/// <summary>
/// Result of a share vehicle operation
/// </summary>
public class ShareVehicleResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SharedWithUserEmail { get; set; }
}
