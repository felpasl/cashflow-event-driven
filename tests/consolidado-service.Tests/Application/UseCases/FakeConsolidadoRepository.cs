using ConsolidadoService.Application;
using ConsolidadoService.Domain;

namespace ConsolidadoService.Tests.Application.UseCases;

internal sealed class FakeConsolidadoRepository : IConsolidadoRepository
{
    public SaldoDiario? SaldoRetornado { get; set; }

    public IReadOnlyList<SaldoDiario> SaldosListados { get; set; } = [];

    public bool ObterSaldoChamado { get; private set; }

    public (Guid MerchantId, DateOnly De, DateOnly Ate)? ListagemCapturada { get; private set; }

    public Task<SaldoDiario?> ObterSaldoAsync(Guid merchantId, DateOnly data)
    {
        ObterSaldoChamado = true;
        return Task.FromResult(SaldoRetornado);
    }

    public Task<IReadOnlyList<SaldoDiario>> ListarSaldosAsync(Guid merchantId, DateOnly de, DateOnly ate)
    {
        ListagemCapturada = (merchantId, de, ate);
        return Task.FromResult(SaldosListados);
    }
}
