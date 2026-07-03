using LancamentosService.Application;
using LancamentosService.Application.UseCases;
using LancamentosService.Contracts;
using LancamentosService.Domain;

namespace LancamentosService.Endpoints;

public static class LancamentosEndpoints
{
    public static RouteGroupBuilder MapLancamentosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/lancamentos");

        group.MapPost("/", async (HttpRequest httpRequest, CreateLancamentoRequest request, CriarLancamentoUseCase useCase, ILogger<Program> logger) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                logger.LogWarning("Requisição de criação de lançamento sem X-Merchant-Id válido.");
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var result = await useCase.ExecutarAsync(merchantId, request);

            return result switch
            {
                CriarLancamentoResult.Sucesso s => Results.Created($"/api/v1/lancamentos/{s.Lancamento.Id}", s.Lancamento),
                CriarLancamentoResult.Invalido i => Results.BadRequest(new { message = i.Mensagem }),
                _ => Results.Problem()
            };
        });

        group.MapGet("/", async (HttpRequest httpRequest, DateOnly? de, DateOnly? ate, TipoLancamento? tipo, string? categoria, int? page, int? pageSize, ListarLancamentosUseCase useCase) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var items = await useCase.ExecutarAsync(merchantId, de, ate, tipo, categoria, page, pageSize);

            return Results.Ok(items);
        });

        group.MapGet("/{id:guid}", async (HttpRequest httpRequest, Guid id, ObterLancamentoUseCase useCase) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var lancamento = await useCase.ExecutarAsync(merchantId, id);

            return lancamento is null ? Results.NotFound() : Results.Ok(lancamento);
        });

        group.MapPost("/{id:guid}/estorno", async (HttpRequest httpRequest, Guid id, EstornarLancamentoUseCase useCase, ILogger<Program> logger) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                logger.LogWarning("Requisição de estorno sem X-Merchant-Id válido para o lançamento {LancamentoId}.", id);
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var result = await useCase.ExecutarAsync(merchantId, id);

            return result switch
            {
                EstornarLancamentoResult.Sucesso s => Results.Created($"/api/v1/lancamentos/{s.Lancamento.Id}", s.Lancamento),
                EstornarLancamentoResult.NaoEncontrado => Results.NotFound(),
                EstornarLancamentoResult.JaEstornado => Results.Conflict(new { message = "Lançamento já foi estornado." }),
                EstornarLancamentoResult.Invalido i => Results.BadRequest(new { message = i.Mensagem }),
                _ => Results.Problem()
            };
        });

        return group;
    }
}
