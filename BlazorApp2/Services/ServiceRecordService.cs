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
    /// Get all service records for a specific vehicle, including their items
    /// </summary>
    public async Task<List<ServiceRecord>> GetServiceRecordsForVehicleAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.CreatedByUser)
            .Include(sr => sr.Items)
            .Where(sr => sr.VehicleId == vehicleId)
            .OrderByDescending(sr => sr.RecordDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get service records filtered by record type
    /// </summary>
    public async Task<List<ServiceRecord>> GetServiceRecordsByTypeAsync(long vehicleId, string recordType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.CreatedByUser)
            .Include(sr => sr.Items)
            .Where(sr => sr.VehicleId == vehicleId && sr.RecordType == recordType)
            .OrderByDescending(sr => sr.RecordDate)
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
            .Include(sr => sr.Items)
            .Where(sr => sr.VehicleId == vehicleId 
                && sr.RecordDate >= startDate 
                && sr.RecordDate <= endDate)
            .OrderByDescending(sr => sr.RecordDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get a single service record by ID, including items
    /// </summary>
    public async Task<ServiceRecord?> GetServiceRecordByIdAsync(long id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.Vehicle)
            .Include(sr => sr.CreatedByUser)
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }

    /// <summary>
    /// Add a new service record with optional items
    /// </summary>
    public async Task<ServiceRecord> AddServiceRecordAsync(ServiceRecord record, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        record.CreatedByUserId = userId;
        record.CreatedAt = DateTime.UtcNow;

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
        
        var existing = await context.ServiceRecords
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == record.Id);
        if (existing == null)
            return null;

        // Only the creator can update
        if (existing.CreatedByUserId != userId)
            return null;

        existing.RecordType = record.RecordType;
        existing.RecordDate = record.RecordDate;
        existing.MilesAtService = record.MilesAtService;
        existing.HoursAtService = record.HoursAtService;
        existing.Title = record.Title;
        existing.Notes = record.Notes;
        existing.ShopName = record.ShopName;
        existing.Technician = record.Technician;
        existing.LaborCost = record.LaborCost;
        existing.PartsCost = record.PartsCost;
        // TotalCost is computed by DB — don't set it

        // Replace items: remove old, add new
        context.ServiceItems.RemoveRange(existing.Items);
        foreach (var item in record.Items)
        {
            item.RecordId = existing.Id;
            item.CreatedAt = DateTime.UtcNow;
            context.ServiceItems.Add(item);
        }

        await context.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// Delete a service record (items cascade-delete via DB FK)
    /// </summary>
    public async Task<bool> DeleteServiceRecordAsync(long id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var record = await context.ServiceRecords.FindAsync(id);
        if (record == null)
            return false;

        // Only the creator can delete
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
            .SumAsync(sr => sr.TotalCost ?? 0);
    }

    /// <summary>
    /// Get maintenance cost breakdown by year
    /// </summary>
    public async Task<Dictionary<int, decimal>> GetCostByYearAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .GroupBy(sr => sr.RecordDate.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(sr => sr.TotalCost ?? 0) })
            .ToDictionaryAsync(x => x.Year, x => x.Total);
    }

    /// <summary>
    /// Get the most recent service record for a vehicle
    /// </summary>
    public async Task<ServiceRecord?> GetMostRecentServiceAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Include(sr => sr.Items)
            .Where(sr => sr.VehicleId == vehicleId)
            .OrderByDescending(sr => sr.RecordDate)
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

    /// <summary>
    /// Get odometer history for a vehicle (all records that have miles_at_service)
    /// </summary>
    public async Task<List<ServiceRecord>> GetOdometerHistoryAsync(long vehicleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId && sr.MilesAtService != null)
            .OrderByDescending(sr => sr.RecordDate)
            .ThenByDescending(sr => sr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all items of a specific category across all records for a vehicle
    /// </summary>
    public async Task<List<ServiceItem>> GetItemsByCategoryAsync(long vehicleId, string category)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ServiceItems
            .Include(si => si.Record)
            .Where(si => si.Record.VehicleId == vehicleId && si.Category == category)
            .OrderByDescending(si => si.Record.RecordDate)
            .ToListAsync();
    }
}
