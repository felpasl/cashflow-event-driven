using LancamentosService.Application;
using LancamentosService.Domain;

namespace LancamentosService.Tests.Application.UseCases;

internal sealed class FakeLancamentosRepository : ILancamentosRepository
{
    public List<(Lancamento Lancamento, OutboxEvent OutboxEvent)> Registrados { get; } = [];

    public Lancamento? LancamentoRetornado { get; set; }

    public bool ExisteEstorno { get; set; }

    public IReadOnlyList<Lancamento> LancamentosListados { get; set; } = [];

    public LancamentoFiltro? FiltroCapturado { get; private set; }

    public Task RegistrarAsync(Lancamento lancamento, OutboxEvent outboxEvent)
    {
        Registrados.Add((lancamento, outboxEvent));
        return Task.CompletedTask;
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, Guid merchantId)
    {
        return Task.FromResult(LancamentoRetornado);
    }

    public Task<bool> ExisteEstornoAsync(Guid lancamentoId, Guid merchantId)
    {
        return Task.FromResult(ExisteEstorno);
    }

    public Task<IReadOnlyList<Lancamento>> ListarAsync(Guid merchantId, LancamentoFiltro filtro)
    {
        FiltroCapturado = filtro;
        return Task.FromResult(LancamentosListados);
    }
}
