using ConsolidadoService.Contracts;

namespace ConsolidadoService.Application.UseCases;

public abstract record ListarConsolidadosResult
{
    public sealed record Sucesso(IReadOnlyList<ConsolidadoResponse> Consolidados) : ListarConsolidadosResult;

    public sealed record Invalido(string Mensagem) : ListarConsolidadosResult;
}

public sealed class ListarConsolidadosUseCase(IConsolidadoRepository repository)
{
    public async Task<ListarConsolidadosResult> ExecutarAsync(Guid merchantId, DateOnly de, DateOnly ate)
    {
        if (ate < de)
        {
            return new ListarConsolidadosResult.Invalido("Parâmetro 'ate' deve ser maior ou igual a 'de'.");
        }

        var saldos = await repository.ListarSaldosAsync(merchantId, de, ate);

        return new ListarConsolidadosResult.Sucesso(saldos.Select(x => x.ToResponse()).ToList());
    }
}
