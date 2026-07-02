using ConsolidadoService.Application;
using ConsolidadoService.Contracts;
using ConsolidadoService.Infrastructure;
using ConsolidadoService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoService.Endpoints;

public static class ConsolidadosEndpoints
{
    public static RouteGroupBuilder MapConsolidadosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/consolidados");

        group.MapGet("/{data}", async (HttpRequest request, DateOnly data, ConsolidadoDbContext dbContext, ConsolidadoCache cache) =>
        {
            if (!MerchantContext.TryGetMerchantId(request, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var cached = await cache.GetAsync(merchantId, data);
            if (cached is not null)
            {
                return Results.Ok(cached);
            }

            var saldo = await dbContext.SaldosDiarios.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.Data == data);

            var response = saldo?.ToResponse() ?? new ConsolidadoResponse(merchantId, data, 0, 0, 0, 0, DateTime.UtcNow);
            await cache.SetAsync(response);
            return Results.Ok(response);
        });

        group.MapGet("/", async (HttpRequest request, DateOnly de, DateOnly ate, ConsolidadoDbContext dbContext) =>
        {
            if (!MerchantContext.TryGetMerchantId(request, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            if (ate < de)
            {
                return Results.BadRequest(new { message = "Parâmetro 'ate' deve ser maior ou igual a 'de'." });
            }

            var items = await dbContext.SaldosDiarios.AsNoTracking()
                .Where(x => x.MerchantId == merchantId && x.Data >= de && x.Data <= ate)
                .OrderBy(x => x.Data)
                .Select(x => x.ToResponse())
                .ToListAsync();

            return Results.Ok(items);
        });

        return group;
    }
}
