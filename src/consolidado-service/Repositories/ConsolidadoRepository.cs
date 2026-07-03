using ConsolidadoService.Application;
using ConsolidadoService.Domain;
using ConsolidadoService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoService.Repositories;

public sealed class ConsolidadoRepository(ConsolidadoDbContext dbContext) : IConsolidadoRepository
{
    public Task<SaldoDiario?> ObterSaldoAsync(Guid merchantId, DateOnly data)
    {
        return dbContext.SaldosDiarios.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.Data == data);
    }

    public async Task<IReadOnlyList<SaldoDiario>> ListarSaldosAsync(Guid merchantId, DateOnly de, DateOnly ate)
    {
        return await dbContext.SaldosDiarios.AsNoTracking()
            .Where(x => x.MerchantId == merchantId && x.Data >= de && x.Data <= ate)
            .OrderBy(x => x.Data)
            .ToListAsync();
    }
}
