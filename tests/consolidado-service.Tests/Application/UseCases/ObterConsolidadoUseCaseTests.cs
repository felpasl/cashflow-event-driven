using ConsolidadoService.Application.UseCases;
using ConsolidadoService.Contracts;
using ConsolidadoService.Domain;

namespace ConsolidadoService.Tests.Application.UseCases;

public sealed class ObterConsolidadoUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Data = new(2026, 7, 2);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarValorDoCacheSemConsultarRepositorio()
    {
        var cacheado = new ConsolidadoResponse(MerchantId, Data, 100, 40, 60, 3, DateTime.UtcNow);
        var repository = new FakeConsolidadoRepository();
        var cache = new FakeConsolidadoCache { Cached = cacheado };
        var useCase = new ObterConsolidadoUseCase(repository, cache);

        var result = await useCase.ExecutarAsync(MerchantId, Data);

        Assert.Equal(cacheado, result);
        Assert.False(repository.ObterSaldoChamado);
        Assert.Empty(cache.Salvos);
    }

    [Fact]
    public async Task ExecutarAsync_DeveConsultarRepositorioECachearQuandoSaldoExiste()
    {
        var saldo = SaldoDiario.Create(MerchantId, Data);
        saldo.Apply(TipoLancamento.Credito, 200m);
        saldo.Apply(TipoLancamento.Debito, 45.50m);

        var repository = new FakeConsolidadoRepository { SaldoRetornado = saldo };
        var cache = new FakeConsolidadoCache();
        var useCase = new ObterConsolidadoUseCase(repository, cache);

        var result = await useCase.ExecutarAsync(MerchantId, Data);

        Assert.Equal(200m, result.TotalCreditos);
        Assert.Equal(45.50m, result.TotalDebitos);
        Assert.Equal(154.50m, result.SaldoDia);
        Assert.True(repository.ObterSaldoChamado);
        var salvo = Assert.Single(cache.Salvos);
        Assert.Equal(result, salvo);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarZeradoECachearQuandoSaldoNaoExiste()
    {
        var repository = new FakeConsolidadoRepository { SaldoRetornado = null };
        var cache = new FakeConsolidadoCache();
        var useCase = new ObterConsolidadoUseCase(repository, cache);

        var result = await useCase.ExecutarAsync(MerchantId, Data);

        Assert.Equal(MerchantId, result.MerchantId);
        Assert.Equal(Data, result.Data);
        Assert.Equal(0, result.TotalCreditos);
        Assert.Equal(0, result.TotalDebitos);
        Assert.Equal(0, result.SaldoDia);
        Assert.Equal(0, result.QuantidadeLancamentos);
        Assert.Single(cache.Salvos);
    }
}
