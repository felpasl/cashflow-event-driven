using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsolidadoService.Persistence;

public sealed class ConsolidadoDbContextFactory : IDesignTimeDbContextFactory<ConsolidadoDbContext>
{
    public ConsolidadoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ConsolidadoDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=consolidado_db;Username=postgres;Password=postgres")
            .Options;

        return new ConsolidadoDbContext(options);
    }
}
