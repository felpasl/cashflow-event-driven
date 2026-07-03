using LancamentosService.Contracts;

namespace LancamentosService.Application.UseCases;

public sealed class ObterLancamentoUseCase(ILancamentosRepository repository)
{
    public async Task<LancamentoResponse?> ExecutarAsync(Guid merchantId, Guid id)
    {
        var lancamento = await repository.ObterPorIdAsync(id, merchantId);
        return lancamento?.ToResponse();
    }
}
