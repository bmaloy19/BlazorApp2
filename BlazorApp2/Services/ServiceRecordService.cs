using BlazorApp2.Data;
using BlazorApp2.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class ServiceRecordService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ServiceRecordService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Get all service records for a specific vehicle
    /// </summary>
    public async Task<List<ServiceRecord>> GetServiceRecordsForVehicleAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.CreatedByUser)
            .Where(sr => sr.VehicleId == vehicleId)
            .OrderByDescending(sr => sr.ServiceDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get service records for a vehicle within a date range
    /// </summary>
    public async Task<List<ServiceRecord>> GetServiceRecordsInRangeAsync(
        long vehicleId, 
        DateOnly startDate, 
        DateOnly endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.CreatedByUser)
            .Where(sr => sr.VehicleId == vehicleId 
                && sr.ServiceDate >= startDate 
                && sr.ServiceDate <= endDate)
            .OrderByDescending(sr => sr.ServiceDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get a single service record by ID
    /// </summary>
    public async Task<ServiceRecord?> GetServiceRecordByIdAsync(long id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.Vehicle)
            .Include(sr => sr.CreatedByUser)
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }

    /// <summary>
    /// Add a new service record
    /// </summary>
    public async Task<ServiceRecord> AddServiceRecordAsync(ServiceRecord record, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        record.CreatedByUserId = userId;
        record.CreatedAt = DateTime.UtcNow;
        
        // Calculate total cost if not provided
        if (record.TotalCost == 0)
        {
            record.TotalCost = record.LaborCost + record.PartsCost;
        }

        context.ServiceRecords.Add(record);
        await context.SaveChangesAsync();
        
        return record;
    }

    /// <summary>
    /// Update an existing service record
    /// </summary>
    public async Task<ServiceRecord?> UpdateServiceRecordAsync(ServiceRecord record, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var existing = await context.ServiceRecords.FindAsync(record.Id);
        if (existing == null)
            return null;

        // Only the creator can update (or add admin check later)
        if (existing.CreatedByUserId != userId)
            return null;

        existing.ServiceDate = record.ServiceDate;
        existing.MilesAtService = record.MilesAtService;
        existing.HoursAtService = record.HoursAtService;
        existing.Title = record.Title;
        existing.Description = record.Description;
        existing.LaborCost = record.LaborCost;
        existing.PartsCost = record.PartsCost;
        existing.TotalCost = record.LaborCost + record.PartsCost;
        existing.ShopName = record.ShopName;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// Delete a service record
    /// </summary>
    public async Task<bool> DeleteServiceRecordAsync(long id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var record = await context.ServiceRecords.FindAsync(id);
        if (record == null)
            return false;

        // Only the creator can delete (or add admin check later)
        if (record.CreatedByUserId != userId)
            return false;

        context.ServiceRecords.Remove(record);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get total maintenance cost for a vehicle
    /// </summary>
    public async Task<decimal> GetTotalMaintenanceCostAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .SumAsync(sr => sr.TotalCost);
    }

    /// <summary>
    /// Get maintenance cost breakdown by year
    /// </summary>
    public async Task<Dictionary<int, decimal>> GetCostByYearAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .GroupBy(sr => sr.ServiceDate.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(sr => sr.TotalCost) })
            .ToDictionaryAsync(x => x.Year, x => x.Total);
    }

    /// <summary>
    /// Get the most recent service record for a vehicle
    /// </summary>
    public async Task<ServiceRecord?> GetMostRecentServiceAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .OrderByDescending(sr => sr.ServiceDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Check if user has access to add service records for a vehicle
    /// </summary>
    public async Task<bool> CanUserAddServiceRecordAsync(string userId, long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // User must have a relationship with the vehicle
        return await context.UserVehicles
            .AnyAsync(uv => uv.UserId == userId && uv.VehicleId == vehicleId);
    }
}
