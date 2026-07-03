using LancamentosService.Application.UseCases;
using LancamentosService.Contracts;
using LancamentosService.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace LancamentosService.Tests.Application.UseCases;

public sealed class CriarLancamentoUseCaseTests
{
    private static readonly Guid MerchantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task ExecutarAsync_DeveRegistrarERetornarSucesso()
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new CriarLancamentoUseCase(repository, NullLogger<CriarLancamentoUseCase>.Instance);
        var request = new CreateLancamentoRequest(TipoLancamento.Credito, 150.75m, Hoje, "Venda cartão", "Vendas");

        var result = await useCase.ExecutarAsync(MerchantId, request);

        var sucesso = Assert.IsType<CriarLancamentoResult.Sucesso>(result);
        Assert.Equal(MerchantId, sucesso.Lancamento.MerchantId);
        Assert.Equal(request.Valor, sucesso.Lancamento.Valor);
        var registrado = Assert.Single(repository.Registrados);
        Assert.Equal(sucesso.Lancamento.Id, registrado.Lancamento.Id);
        Assert.Equal("LancamentoRegistrado", registrado.OutboxEvent.EventType);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarInvalidoQuandoValorNaoPositivo()
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new CriarLancamentoUseCase(repository, NullLogger<CriarLancamentoUseCase>.Instance);
        var request = new CreateLancamentoRequest(TipoLancamento.Credito, 0, Hoje, "Venda cartão", null);

        var result = await useCase.ExecutarAsync(MerchantId, request);

        var invalido = Assert.IsType<CriarLancamentoResult.Invalido>(result);
        Assert.Contains("Valor", invalido.Mensagem);
        Assert.Empty(repository.Registrados);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarInvalidoQuandoDataFutura()
    {
        var repository = new FakeLancamentosRepository();
        var useCase = new CriarLancamentoUseCase(repository, NullLogger<CriarLancamentoUseCase>.Instance);
        var request = new CreateLancamentoRequest(TipoLancamento.Debito, 10, Hoje.AddDays(1), "Compra", null);

        var result = await useCase.ExecutarAsync(MerchantId, request);

        var invalido = Assert.IsType<CriarLancamentoResult.Invalido>(result);
        Assert.Contains("futura", invalido.Mensagem);
        Assert.Empty(repository.Registrados);
    }
}
