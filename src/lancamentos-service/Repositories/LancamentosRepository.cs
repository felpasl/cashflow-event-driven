using LancamentosService.Application;
using LancamentosService.Domain;
using LancamentosService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LancamentosService.Repositories;

public sealed class LancamentosRepository(LancamentosDbContext dbContext) : ILancamentosRepository
{
    public async Task RegistrarAsync(Lancamento lancamento, OutboxEvent outboxEvent)
    {
        dbContext.Lancamentos.Add(lancamento);
        dbContext.OutboxEvents.Add(outboxEvent);
        await dbContext.SaveChangesAsync();
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, Guid merchantId)
    {
        return dbContext.Lancamentos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId);
    }

    public Task<bool> ExisteEstornoAsync(Guid lancamentoId, Guid merchantId)
    {
        return dbContext.Lancamentos
            .AnyAsync(x => x.EstornoDoLancamentoId == lancamentoId && x.MerchantId == merchantId);
    }

    public async Task<IReadOnlyList<Lancamento>> ListarAsync(Guid merchantId, LancamentoFiltro filtro)
    {
        var query = dbContext.Lancamentos.AsNoTracking().Where(x => x.MerchantId == merchantId);

        if (filtro.De.HasValue)
        {
            query = query.Where(x => x.DataLancamento >= filtro.De.Value);
        }

        if (filtro.Ate.HasValue)
        {
            query = query.Where(x => x.DataLancamento <= filtro.Ate.Value);
        }

        if (filtro.Tipo.HasValue)
        {
            query = query.Where(x => x.Tipo == filtro.Tipo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            query = query.Where(x => x.Categoria == filtro.Categoria);
        }

        return await query
            .OrderByDescending(x => x.DataLancamento)
            .ThenByDescending(x => x.CriadoEm)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();
    }
}
