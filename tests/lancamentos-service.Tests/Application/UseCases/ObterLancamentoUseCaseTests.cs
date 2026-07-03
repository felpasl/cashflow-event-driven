using LancamentosService.Application.UseCases;
using LancamentosService.Domain;

namespace LancamentosService.Tests.Application.UseCases;

public sealed class ObterLancamentoUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarNullQuandoNaoEncontrado()
    {
        var repository = new FakeLancamentosRepository { LancamentoRetornado = null };
        var useCase = new ObterLancamentoUseCase(repository);

        var result = await useCase.ExecutarAsync(MerchantId, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarResponseQuandoEncontrado()
    {
        var lancamento = Lancamento.Criar(MerchantId, TipoLancamento.Debito, 25, Hoje, "Compra", "Insumos", Hoje);
        var repository = new FakeLancamentosRepository { LancamentoRetornado = lancamento };
        var useCase = new ObterLancamentoUseCase(repository);

        var result = await useCase.ExecutarAsync(MerchantId, lancamento.Id);

        Assert.NotNull(result);
        Assert.Equal(lancamento.Id, result!.Id);
        Assert.Equal(lancamento.Categoria, result.Categoria);
    }
}
