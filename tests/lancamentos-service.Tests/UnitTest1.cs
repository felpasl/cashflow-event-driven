using System.Text.Json;
using LancamentosService.Application;
using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Tests;

public sealed class LancamentoTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Hoje = new(2026, 7, 2);

    [Fact]
    public void Criar_DeveRejeitarValorNaoPositivo()
    {
        var exception = Assert.Throws<DomainException>(() => Lancamento.Criar(
            MerchantId,
            TipoLancamento.Credito,
            0,
            Hoje,
            "Venda",
            null,
            Hoje));

        Assert.Contains("Valor", exception.Message);
    }

    [Fact]
    public void Criar_DeveRejeitarDataFutura()
    {
        var exception = Assert.Throws<DomainException>(() => Lancamento.Criar(
            MerchantId,
            TipoLancamento.Credito,
            10,
            Hoje.AddDays(1),
            "Venda",
            null,
            Hoje));

        Assert.Contains("futura", exception.Message);
    }

    [Fact]
    public void Criar_DeveAceitarDataRetroativa()
    {
        var lancamento = Lancamento.Criar(
            MerchantId,
            TipoLancamento.Credito,
            10,
            Hoje.AddDays(-1),
            "Venda",
            null,
            Hoje);

        Assert.Equal(Hoje.AddDays(-1), lancamento.DataLancamento);
    }

    [Fact]
    public void Estornar_DeveCriarLancamentoOposto()
    {
        var original = Lancamento.Criar(
            MerchantId,
            TipoLancamento.Credito,
            150.75m,
            Hoje,
            "Venda cartão",
            "Vendas",
            Hoje);

        var estorno = original.Estornar(Hoje);

        Assert.Equal(TipoLancamento.Debito, estorno.Tipo);
        Assert.Equal(original.Valor, estorno.Valor);
        Assert.Equal(original.Id, estorno.EstornoDoLancamentoId);
        Assert.Equal(original.MerchantId, estorno.MerchantId);
    }

    [Fact]
    public void ToOutboxEvent_DeveCriarEventoVersionado()
    {
        var lancamento = Lancamento.Criar(
            MerchantId,
            TipoLancamento.Credito,
            150.75m,
            Hoje,
            "Venda cartão",
            "Vendas",
            Hoje);

        var outboxEvent = lancamento.ToOutboxEvent();
        var payload = JsonSerializer.Deserialize<LancamentoRegistradoPayload>(outboxEvent.Payload);

        Assert.Equal("LancamentoRegistrado", outboxEvent.EventType);
        Assert.Equal(1, outboxEvent.EventVersion);
        Assert.Equal(OutboxStatus.Pending, outboxEvent.Status);
        Assert.NotNull(payload);
        Assert.Equal(lancamento.Id, payload!.LancamentoId);
        Assert.Equal(lancamento.MerchantId, payload.MerchantId);
    }
}
