using LancamentosService.Domain;

namespace LancamentosService.Application;

public sealed record LancamentoFiltro(
    DateOnly? De,
    DateOnly? Ate,
    TipoLancamento? Tipo,
    string? Categoria,
    int Page,
    int PageSize);
