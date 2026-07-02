using System.Text.Json;
using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Application;

public static class LancamentoMapper
{
    public static LancamentoResponse ToResponse(this Lancamento lancamento)
    {
        return new LancamentoResponse(
            lancamento.Id,
            lancamento.MerchantId,
            lancamento.Tipo,
            lancamento.Valor,
            lancamento.DataLancamento,
            lancamento.Descricao,
            lancamento.Categoria,
            lancamento.EstornoDoLancamentoId,
            lancamento.CriadoEm);
    }

    public static OutboxEvent ToOutboxEvent(this Lancamento lancamento)
    {
        var payload = new LancamentoRegistradoPayload(
            lancamento.MerchantId,
            lancamento.Id,
            lancamento.Tipo,
            lancamento.Valor,
            lancamento.DataLancamento,
            lancamento.Descricao,
            lancamento.Categoria,
            lancamento.EstornoDoLancamentoId);

        return OutboxEvent.Create(
            "LancamentoRegistrado",
            1,
            JsonSerializer.Serialize(payload));
    }
}
