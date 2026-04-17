using BlazorApp2.Data.Models;

namespace BlazorApp2.Services;

/// <summary>
/// Per-circuit (scoped) cache for the current user's garage data.
/// Eliminates redundant DB round-trips when navigating between
/// Garage ↔ VehicleDetails within the same SignalR circuit.
/// </summary>
public class GarageStateProvider
{
    private readonly VehicleService _vehicleService;
    private readonly ServiceRecordService _serviceRecordService;

    // ── Vehicle list cache ──
    private List<UserVehicle>? _ownedVehicles;
    private List<UserVehicle>? _sharedVehicles;
    private string? _cachedUserId;

    // ── Vehicle detail caches (keyed by vehicleId) ──
    private readonly Dictionary<long, Vehicle> _vehicleCache = new();
    private readonly Dictionary<long, List<ServiceRecord>> _serviceRecordCache = new();

    /// <summary>
    /// Raised whenever cached data is invalidated so subscribing
    /// components can re-render if needed.
    /// </summary>
    public event Action? OnChange;

    public GarageStateProvider(
        VehicleService vehicleService,
        ServiceRecordService serviceRecordService)
    {
        _vehicleService = vehicleService;
        _serviceRecordService = serviceRecordService;
    }

    // ────────────────────────────────────────────────────────
    // Vehicle Lists
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the user's owned vehicles (cached after first call).
    /// </summary>
    public async Task<List<UserVehicle>> GetOwnedVehiclesAsync(string userId)
    {
        EnsureUserContext(userId);

        if (_ownedVehicles is not null)
            return _ownedVehicles;

        _ownedVehicles = await _vehicleService.GetOwnedVehiclesAsync(userId);
        return _ownedVehicles;
    }

    /// <summary>
    /// Returns vehicles shared with the user (cached after first call).
    /// </summary>
    public async Task<List<UserVehicle>> GetSharedVehiclesAsync(string userId)
    {
        EnsureUserContext(userId);

        if (_sharedVehicles is not null)
            return _sharedVehicles;

        _sharedVehicles = await _vehicleService.GetSharedVehiclesAsync(userId);
        return _sharedVehicles;
    }

    // ────────────────────────────────────────────────────────
    // Vehicle Details
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a vehicle by ID (cached after first call).
    /// </summary>
    public async Task<Vehicle?> GetVehicleDetailsAsync(long vehicleId)
    {
        if (_vehicleCache.TryGetValue(vehicleId, out var cached))
            return cached;

        var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
        if (vehicle is not null)
            _vehicleCache[vehicleId] = vehicle;

        return vehicle;
    }

    // ────────────────────────────────────────────────────────
    // Service Records
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns service records for a vehicle (cached after first call).
    /// </summary>
    public async Task<List<ServiceRecord>> GetServiceRecordsAsync(long vehicleId)
    {
        if (_serviceRecordCache.TryGetValue(vehicleId, out var cached))
            return cached;

        var records = await _serviceRecordService.GetServiceRecordsForVehicleAsync(vehicleId);
        _serviceRecordCache[vehicleId] = records;
        return records;
    }

    // ────────────────────────────────────────────────────────
    // Preload (fire-and-forget from Home page)
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Eagerly fetches owned and shared vehicles so the Garage page
    /// renders instantly on first visit.
    /// </summary>
    public async Task PreloadAsync(string userId)
    {
        EnsureUserContext(userId);

        // Fetch both lists concurrently
        var ownedTask = _ownedVehicles is null
            ? _vehicleService.GetOwnedVehiclesAsync(userId)
            : Task.FromResult(_ownedVehicles);

        var sharedTask = _sharedVehicles is null
            ? _vehicleService.GetSharedVehiclesAsync(userId)
            : Task.FromResult(_sharedVehicles);

        _ownedVehicles = await ownedTask;
        _sharedVehicles = await sharedTask;
    }

    // ────────────────────────────────────────────────────────
    // Cache Invalidation
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the owned/shared vehicle list cache.
    /// Call after adding or deleting a vehicle.
    /// </summary>
    public void InvalidateVehicleList()
    {
        _ownedVehicles = null;
        _sharedVehicles = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the cached detail for a specific vehicle.
    /// Call after editing vehicle properties.
    /// </summary>
    public void InvalidateVehicleDetails(long vehicleId)
    {
        _vehicleCache.Remove(vehicleId);
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the cached service records for a specific vehicle.
    /// Call after adding, editing, or deleting a service record.
    /// </summary>
    public void InvalidateServiceRecords(long vehicleId)
    {
        _serviceRecordCache.Remove(vehicleId);
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all caches. Useful after major operations like
    /// deleting a vehicle.
    /// </summary>
    public void InvalidateAll()
    {
        _ownedVehicles = null;
        _sharedVehicles = null;
        _vehicleCache.Clear();
        _serviceRecordCache.Clear();
        NotifyStateChanged();
    }

    // ────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// If the user ID changes (shouldn't happen within a circuit, but
    /// defensive), clear all caches.
    /// </summary>
    private void EnsureUserContext(string userId)
    {
        if (_cachedUserId is not null && _cachedUserId != userId)
        {
            // Different user — clear everything
            InvalidateAll();
        }

        _cachedUserId = userId;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
