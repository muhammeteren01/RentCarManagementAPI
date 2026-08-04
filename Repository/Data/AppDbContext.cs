using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<DamageReport> DamageReports => Set<DamageReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
