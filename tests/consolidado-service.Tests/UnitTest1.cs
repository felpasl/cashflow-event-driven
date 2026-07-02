using ConsolidadoService.Domain;

namespace ConsolidadoService.Tests;

public sealed class SaldoDiarioTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Data = new(2026, 7, 2);

    [Fact]
    public void Apply_Credito_DeveIncrementarTotalCreditosESaldo()
    {
        var saldo = SaldoDiario.Create(MerchantId, Data);

        saldo.Apply(TipoLancamento.Credito, 150.75m);

        Assert.Equal(150.75m, saldo.TotalCreditos);
        Assert.Equal(0, saldo.TotalDebitos);
        Assert.Equal(150.75m, saldo.SaldoDia);
        Assert.Equal(1, saldo.QuantidadeLancamentos);
    }

    [Fact]
    public void Apply_Debito_DeveIncrementarTotalDebitosEDiminuirSaldo()
    {
        var saldo = SaldoDiario.Create(MerchantId, Data);

        saldo.Apply(TipoLancamento.Debito, 40.25m);

        Assert.Equal(0, saldo.TotalCreditos);
        Assert.Equal(40.25m, saldo.TotalDebitos);
        Assert.Equal(-40.25m, saldo.SaldoDia);
        Assert.Equal(1, saldo.QuantidadeLancamentos);
    }

    [Fact]
    public void Apply_CreditoEDebito_DeveCalcularSaldoDoDia()
    {
        var saldo = SaldoDiario.Create(MerchantId, Data);

        saldo.Apply(TipoLancamento.Credito, 200m);
        saldo.Apply(TipoLancamento.Debito, 45.50m);

        Assert.Equal(200m, saldo.TotalCreditos);
        Assert.Equal(45.50m, saldo.TotalDebitos);
        Assert.Equal(154.50m, saldo.SaldoDia);
        Assert.Equal(2, saldo.QuantidadeLancamentos);
    }

    [Fact]
    public void Apply_DeveRejeitarValorNaoPositivo()
    {
        var saldo = SaldoDiario.Create(MerchantId, Data);

        var exception = Assert.Throws<InvalidOperationException>(() => saldo.Apply(TipoLancamento.Credito, 0));

        Assert.Contains("maior que zero", exception.Message);
    }

    [Fact]
    public void ProcessedEvent_DeveRegistrarEventoProcessado()
    {
        var eventId = Guid.NewGuid();
        var processed = ProcessedEvent.Create(eventId, "1680000000000-0", "LancamentoRegistrado");

        Assert.Equal(eventId, processed.EventId);
        Assert.Equal("1680000000000-0", processed.StreamMessageId);
        Assert.Equal("LancamentoRegistrado", processed.EventType);
    }
}
