using LancamentosService.Application.UseCases;
using LancamentosService.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace LancamentosService.Tests.Application.UseCases;

public sealed class EstornarLancamentoUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarNaoEncontradoQuandoLancamentoNaoExiste()
    {
        var repository = new FakeLancamentosRepository { LancamentoRetornado = null };
        var useCase = new EstornarLancamentoUseCase(repository, NullLogger<EstornarLancamentoUseCase>.Instance);

        var result = await useCase.ExecutarAsync(MerchantId, Guid.NewGuid());

        Assert.IsType<EstornarLancamentoResult.NaoEncontrado>(result);
        Assert.Empty(repository.Registrados);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarJaEstornadoQuandoJaExisteEstorno()
    {
        var original = Lancamento.Criar(MerchantId, TipoLancamento.Credito, 100, Hoje, "Venda", null, Hoje);
        var repository = new FakeLancamentosRepository { LancamentoRetornado = original, ExisteEstorno = true };
        var useCase = new EstornarLancamentoUseCase(repository, NullLogger<EstornarLancamentoUseCase>.Instance);

        var result = await useCase.ExecutarAsync(MerchantId, original.Id);

        Assert.IsType<EstornarLancamentoResult.JaEstornado>(result);
        Assert.Empty(repository.Registrados);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRegistrarEstornoERetornarSucesso()
    {
        var original = Lancamento.Criar(MerchantId, TipoLancamento.Credito, 100, Hoje, "Venda", "Categoria", Hoje);
        var repository = new FakeLancamentosRepository { LancamentoRetornado = original, ExisteEstorno = false };
        var useCase = new EstornarLancamentoUseCase(repository, NullLogger<EstornarLancamentoUseCase>.Instance);

        var result = await useCase.ExecutarAsync(MerchantId, original.Id);

        var sucesso = Assert.IsType<EstornarLancamentoResult.Sucesso>(result);
        Assert.Equal(TipoLancamento.Debito, sucesso.Lancamento.Tipo);
        Assert.Equal(original.Id, sucesso.Lancamento.EstornoDoLancamentoId);
        var registrado = Assert.Single(repository.Registrados);
        Assert.Equal(sucesso.Lancamento.Id, registrado.Lancamento.Id);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarInvalidoQuandoLancamentoJaEEstorno()
    {
        var original = Lancamento.Criar(MerchantId, TipoLancamento.Credito, 100, Hoje, "Venda", null, Hoje);
        var estornoExistente = original.Estornar(Hoje);
        var repository = new FakeLancamentosRepository { LancamentoRetornado = estornoExistente, ExisteEstorno = false };
        var useCase = new EstornarLancamentoUseCase(repository, NullLogger<EstornarLancamentoUseCase>.Instance);

        var result = await useCase.ExecutarAsync(MerchantId, estornoExistente.Id);

        var invalido = Assert.IsType<EstornarLancamentoResult.Invalido>(result);
        Assert.Contains("estornado novamente", invalido.Mensagem);
        Assert.Empty(repository.Registrados);
    }
}
