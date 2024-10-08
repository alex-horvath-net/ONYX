using Common.Infrastucture.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastucture.Data;
public class AppDB(DbContextOptions<AppDB> options) : DbContext(options) {
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Specify column types if necessary
        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");

        base.OnModelCreating(modelBuilder);
    }
}


