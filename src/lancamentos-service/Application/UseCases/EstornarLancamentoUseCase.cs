using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Application.UseCases;

public abstract record EstornarLancamentoResult
{
    public sealed record Sucesso(LancamentoResponse Lancamento) : EstornarLancamentoResult;

    public sealed record NaoEncontrado : EstornarLancamentoResult;

    public sealed record JaEstornado : EstornarLancamentoResult;

    public sealed record Invalido(string Mensagem) : EstornarLancamentoResult;
}

public sealed class EstornarLancamentoUseCase(ILancamentosRepository repository, ILogger<EstornarLancamentoUseCase> logger)
{
    public async Task<EstornarLancamentoResult> ExecutarAsync(Guid merchantId, Guid id)
    {
        var original = await repository.ObterPorIdAsync(id, merchantId);
        if (original is null)
        {
            logger.LogWarning("Tentativa de estornar lançamento inexistente {LancamentoId} para o merchant {MerchantId}.", id, merchantId);
            return new EstornarLancamentoResult.NaoEncontrado();
        }

        var alreadyReversed = await repository.ExisteEstornoAsync(id, merchantId);
        if (alreadyReversed)
        {
            logger.LogWarning("Tentativa de estornar lançamento {LancamentoId} já estornado para o merchant {MerchantId}.", id, merchantId);
            return new EstornarLancamentoResult.JaEstornado();
        }

        try
        {
            var estorno = original.Estornar(DateOnly.FromDateTime(DateTime.UtcNow));
            await repository.RegistrarAsync(estorno, estorno.ToOutboxEvent());

            logger.LogInformation(
                "Estorno {EstornoId} registrado para o lançamento {LancamentoId} do merchant {MerchantId}.",
                estorno.Id, id, merchantId);

            return new EstornarLancamentoResult.Sucesso(estorno.ToResponse());
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Falha de domínio ao estornar lançamento {LancamentoId} do merchant {MerchantId}.", id, merchantId);
            return new EstornarLancamentoResult.Invalido(ex.Message);
        }
    }
}
