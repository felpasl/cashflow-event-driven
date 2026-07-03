using LancamentosService.Domain;

namespace LancamentosService.Application;

public interface ILancamentosRepository
{
    Task RegistrarAsync(Lancamento lancamento, OutboxEvent outboxEvent);

    Task<Lancamento?> ObterPorIdAsync(Guid id, Guid merchantId);

    Task<bool> ExisteEstornoAsync(Guid lancamentoId, Guid merchantId);

    Task<IReadOnlyList<Lancamento>> ListarAsync(Guid merchantId, LancamentoFiltro filtro);
}
