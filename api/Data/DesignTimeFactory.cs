using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ListingsApi.Data;

// Нужна только команде «dotnet ef migrations add …».
// Миграции всегда создаются под PostgreSQL (он стоит на Render), подключение к базе при этом не открывается,
// поэтому запускать PostgreSQL для создания миграции не нужно.
public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=listings;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
