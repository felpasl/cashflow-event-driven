using ConsolidadoService.Application;
using ConsolidadoService.Application.UseCases;

namespace ConsolidadoService.Endpoints;

public static class ConsolidadosEndpoints
{
    public static RouteGroupBuilder MapConsolidadosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/consolidados");

        group.MapGet("/{data}", async (HttpRequest request, DateOnly data, ObterConsolidadoUseCase useCase) =>
        {
            if (!MerchantContext.TryGetMerchantId(request, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var response = await useCase.ExecutarAsync(merchantId, data);

            return Results.Ok(response);
        });

        group.MapGet("/", async (HttpRequest request, DateOnly de, DateOnly ate, ListarConsolidadosUseCase useCase) =>
        {
            if (!MerchantContext.TryGetMerchantId(request, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var result = await useCase.ExecutarAsync(merchantId, de, ate);

            return result switch
            {
                ListarConsolidadosResult.Sucesso s => Results.Ok(s.Consolidados),
                ListarConsolidadosResult.Invalido i => Results.BadRequest(new { message = i.Mensagem }),
                _ => Results.Problem()
            };
        });

        return group;
    }
}
