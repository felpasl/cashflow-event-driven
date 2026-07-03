using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Application.UseCases;

public sealed class ListarLancamentosUseCase(ILancamentosRepository repository)
{
    public async Task<IReadOnlyList<LancamentoResponse>> ExecutarAsync(
        Guid merchantId,
        DateOnly? de,
        DateOnly? ate,
        TipoLancamento? tipo,
        string? categoria,
        int? page,
        int? pageSize)
    {
        var filtro = new LancamentoFiltro(
            de,
            ate,
            tipo,
            categoria,
            Math.Max(page ?? 1, 1),
            Math.Clamp(pageSize ?? 20, 1, 100));

        var items = await repository.ListarAsync(merchantId, filtro);

        return items.Select(x => x.ToResponse()).ToList();
    }
}
