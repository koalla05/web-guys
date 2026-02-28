using InstantWellness.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstantWellness.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subtotal).HasPrecision(18, 4);
            entity.Property(e => e.CompositeTaxRate).HasPrecision(18, 6);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 4);
            entity.Property(e => e.StateRate).HasPrecision(18, 6);
            entity.Property(e => e.CountyRate).HasPrecision(18, 6);
            entity.Property(e => e.CityRate).HasPrecision(18, 6);
            entity.Property(e => e.SpecialRates).HasPrecision(18, 6);
        });
    }
}
