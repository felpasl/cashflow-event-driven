using LancamentosService.Domain;

namespace LancamentosService.Contracts;

public sealed record LancamentoRegistradoPayload(
    Guid MerchantId,
    Guid LancamentoId,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string Descricao,
    string? Categoria,
    Guid? EstornoDoLancamentoId);
