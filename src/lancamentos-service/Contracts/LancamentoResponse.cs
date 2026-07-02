using LancamentosService.Domain;

namespace LancamentosService.Contracts;

public sealed record LancamentoResponse(
    Guid Id,
    Guid MerchantId,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string Descricao,
    string? Categoria,
    Guid? EstornoDoLancamentoId,
    DateTime CriadoEm);
