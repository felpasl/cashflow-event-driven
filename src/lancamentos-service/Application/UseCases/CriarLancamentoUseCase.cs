using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Application.UseCases;

public abstract record CriarLancamentoResult
{
    public sealed record Sucesso(LancamentoResponse Lancamento) : CriarLancamentoResult;

    public sealed record Invalido(string Mensagem) : CriarLancamentoResult;
}

public sealed class CriarLancamentoUseCase(ILancamentosRepository repository, ILogger<CriarLancamentoUseCase> logger)
{
    public async Task<CriarLancamentoResult> ExecutarAsync(Guid merchantId, CreateLancamentoRequest request)
    {
        try
        {
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var lancamento = Lancamento.Criar(
                merchantId,
                request.Tipo,
                request.Valor,
                request.DataLancamento,
                request.Descricao,
                request.Categoria,
                hoje);

            await repository.RegistrarAsync(lancamento, lancamento.ToOutboxEvent());

            logger.LogInformation(
                "Lançamento {LancamentoId} registrado para o merchant {MerchantId}: {Tipo} de {Valor} em {DataLancamento}.",
                lancamento.Id, merchantId, lancamento.Tipo, lancamento.Valor, lancamento.DataLancamento);

            return new CriarLancamentoResult.Sucesso(lancamento.ToResponse());
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Falha de domínio ao registrar lançamento para o merchant {MerchantId}.", merchantId);
            return new CriarLancamentoResult.Invalido(ex.Message);
        }
    }
}
