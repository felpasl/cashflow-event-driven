using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LancamentosService.Persistence;

public sealed class LancamentosDbContextFactory : IDesignTimeDbContextFactory<LancamentosDbContext>
{
    public LancamentosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LancamentosDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=lancamentos_db;Username=postgres;Password=postgres")
            .Options;

        return new LancamentosDbContext(options);
    }
}
