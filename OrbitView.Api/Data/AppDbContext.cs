using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Models;

namespace OrbitView.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<SatelliteCategory> SatelliteCategories => Set<SatelliteCategory>();
    public DbSet<TleRecord> TleRecords => Set<TleRecord>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Favourite> Favourites => Set<Favourite>();
    public DbSet<TleFetchLog> TleFetchLogs => Set<TleFetchLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraints
        modelBuilder.Entity<Satellite>()
            .HasIndex(s => s.NoradId).IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();

        modelBuilder.Entity<SatelliteCategory>()
            .HasIndex(c => c.Slug).IsUnique();

        // Composite unique key on Favourites
        modelBuilder.Entity<Favourite>()
            .HasIndex(f => new { f.UserId, f.SatelliteId }).IsUnique();

        // Decimal precision
        modelBuilder.Entity<TleRecord>()
            .Property(t => t.Inclination).HasPrecision(8, 4);

        modelBuilder.Entity<TleRecord>()
            .Property(t => t.Eccentricity).HasPrecision(10, 7);

        modelBuilder.Entity<TleRecord>()
            .Property(t => t.MeanMotion).HasPrecision(12, 8);

        modelBuilder.Entity<User>()
            .Property(u => u.LocationLat).HasPrecision(9, 6);

        modelBuilder.Entity<User>()
            .Property(u => u.LocationLon).HasPrecision(9, 6);
    }
}