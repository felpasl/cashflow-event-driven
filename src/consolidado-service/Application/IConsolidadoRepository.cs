using ConsolidadoService.Domain;

namespace ConsolidadoService.Application;

public interface IConsolidadoRepository
{
    Task<SaldoDiario?> ObterSaldoAsync(Guid merchantId, DateOnly data);

    Task<IReadOnlyList<SaldoDiario>> ListarSaldosAsync(Guid merchantId, DateOnly de, DateOnly ate);
}
