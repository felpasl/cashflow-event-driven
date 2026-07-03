using LancamentosService.Application.UseCases;
using LancamentosService.Domain;

namespace LancamentosService.Tests.Application.UseCases;

public sealed class ListarLancamentosUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_DeveAplicarPaginacaoPadraoQuandoNaoInformada()
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new ListarLancamentosUseCase(repository);

        await useCase.ExecutarAsync(MerchantId, null, null, null, null, null, null);

        Assert.Equal(1, repository.FiltroCapturado!.Page);
        Assert.Equal(20, repository.FiltroCapturado!.PageSize);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public async Task ExecutarAsync_DeveNormalizarPage(int pageInformado, int pageEsperado)
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new ListarLancamentosUseCase(repository);

        await useCase.ExecutarAsync(MerchantId, null, null, null, null, pageInformado, null);

        Assert.Equal(pageEsperado, repository.FiltroCapturado!.Page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 100)]
    [InlineData(50, 50)]
    public async Task ExecutarAsync_DeveClampPageSize(int pageSizeInformado, int pageSizeEsperado)
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new ListarLancamentosUseCase(repository);

        await useCase.ExecutarAsync(MerchantId, null, null, null, null, null, pageSizeInformado);

        Assert.Equal(pageSizeEsperado, repository.FiltroCapturado!.PageSize);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRepassarFiltrosParaRepositorio()
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new ListarLancamentosUseCase(repository);

        await useCase.ExecutarAsync(MerchantId, Hoje.AddDays(-10), Hoje, TipoLancamento.Debito, "Insumos", 2, 10);

        var filtro = repository.FiltroCapturado!;
        Assert.Equal(Hoje.AddDays(-10), filtro.De);
        Assert.Equal(Hoje, filtro.Ate);
        Assert.Equal(TipoLancamento.Debito, filtro.Tipo);
        Assert.Equal("Insumos", filtro.Categoria);
        Assert.Equal(2, filtro.Page);
        Assert.Equal(10, filtro.PageSize);
    }

    [Fact]
    public async Task ExecutarAsync_DeveMapearLancamentosParaResponse()
    {
        var lancamento = Lancamento.Criar(MerchantId, TipoLancamento.Credito, 10, Hoje, "Venda", null, Hoje);
        var repository = new FakeLancamentosRepository { LancamentosListados = [lancamento] };
        var useCase = new ListarLancamentosUseCase(repository);

        var items = await useCase.ExecutarAsync(MerchantId, null, null, null, null, null, null);

        var item = Assert.Single(items);
        Assert.Equal(lancamento.Id, item.Id);
        Assert.Equal(lancamento.Valor, item.Valor);
    }
}
