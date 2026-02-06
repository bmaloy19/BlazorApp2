using BlazorApp2.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Make> Makes { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<UserVehicle> UserVehicles { get; set; }
    public DbSet<ServiceRecord> ServiceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure composite primary key for UserVehicle
        builder.Entity<UserVehicle>()
            .HasKey(uv => new { uv.UserId, uv.VehicleId });

        // Configure Make entity
        builder.Entity<Make>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.ManufacturerCode);
        });

        // Configure Vehicle entity
        builder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.Vin); // Not unique - different users can have same VIN
            entity.HasIndex(e => e.MakeId);
            entity.HasIndex(e => e.OriginalOwnerUserId);

            entity.HasOne(v => v.Make)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.MakeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.OriginalOwner)
                .WithMany(u => u.OwnedVehicles)
                .HasForeignKey(v => v.OriginalOwnerUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserVehicle entity
        builder.Entity<UserVehicle>(entity =>
        {
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => new { e.UserId, e.IsPrimary });

            entity.HasOne(uv => uv.User)
                .WithMany(u => u.UserVehicles)
                .HasForeignKey(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uv => uv.Vehicle)
                .WithMany(v => v.UserVehicles)
                .HasForeignKey(uv => uv.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceRecord entity
        builder.Entity<ServiceRecord>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleId, e.ServiceDate });
            entity.HasIndex(e => new { e.VehicleId, e.MilesAtService });
            entity.HasIndex(e => new { e.VehicleId, e.HoursAtService });
            entity.HasIndex(e => e.CreatedByUserId);

            entity.Property(e => e.LaborCost).HasPrecision(10, 2);
            entity.Property(e => e.PartsCost).HasPrecision(10, 2);
            entity.Property(e => e.TotalCost).HasPrecision(10, 2);

            entity.HasOne(sr => sr.Vehicle)
                .WithMany(v => v.ServiceRecords)
                .HasForeignKey(sr => sr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.CreatedByUser)
                .WithMany(u => u.ServiceRecords)
                .HasForeignKey(sr => sr.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}