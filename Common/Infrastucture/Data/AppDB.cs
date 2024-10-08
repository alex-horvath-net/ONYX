using Common.Infrastucture.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastucture.Data;
public class AppDB(DbContextOptions<AppDB> options) : DbContext(options) {
    public DbSet<Product> Products { get; set; }
}


