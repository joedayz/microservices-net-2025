using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductService.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();

        // Connection string para desarrollo local
        var connectionString = "Host=localhost;Port=5432;Database=microservices_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new ProductDbContext(optionsBuilder.Options);
    }
}
