using ConsolidadoService.Application.UseCases;
using ConsolidadoService.Domain;

namespace ConsolidadoService.Tests.Application.UseCases;

public sealed class ListarConsolidadosUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly De = new(2026, 7, 1);
    private static readonly DateOnly Ate = new(2026, 7, 10);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarInvalidoQuandoAteMenorQueDe()
    {
        var repository = new FakeConsolidadoRepository();
        var useCase = new ListarConsolidadosUseCase(repository);

        var result = await useCase.ExecutarAsync(MerchantId, Ate, De);

        var invalido = Assert.IsType<ListarConsolidadosResult.Invalido>(result);
        Assert.Contains("'ate'", invalido.Mensagem);
        Assert.Null(repository.ListagemCapturada);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRepassarIntervaloParaRepositorio()
    {
        var repository = new FakeConsolidadoRepository();
        var useCase = new ListarConsolidadosUseCase(repository);

        await useCase.ExecutarAsync(MerchantId, De, Ate);

        Assert.Equal((MerchantId, De, Ate), repository.ListagemCapturada);
    }

    [Fact]
    public async Task ExecutarAsync_DeveMapearSaldosParaResponse()
    {
        var saldo = SaldoDiario.Create(MerchantId, De);
        saldo.Apply(TipoLancamento.Credito, 50m);

        var repository = new FakeConsolidadoRepository { SaldosListados = [saldo] };
        var useCase = new ListarConsolidadosUseCase(repository);

        var result = await useCase.ExecutarAsync(MerchantId, De, Ate);

        var sucesso = Assert.IsType<ListarConsolidadosResult.Sucesso>(result);
        var item = Assert.Single(sucesso.Consolidados);
        Assert.Equal(saldo.MerchantId, item.MerchantId);
        Assert.Equal(saldo.SaldoDia, item.SaldoDia);
    }
}
