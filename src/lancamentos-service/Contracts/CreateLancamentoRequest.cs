using LancamentosService.Domain;

namespace LancamentosService.Contracts;

public sealed record CreateLancamentoRequest(
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string Descricao,
    string? Categoria);
