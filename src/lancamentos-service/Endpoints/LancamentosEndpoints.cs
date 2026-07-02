using LancamentosService.Application;
using LancamentosService.Contracts;
using LancamentosService.Domain;
using LancamentosService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LancamentosService.Endpoints;

public static class LancamentosEndpoints
{
    public static RouteGroupBuilder MapLancamentosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/lancamentos");

        group.MapPost("/", async (HttpRequest httpRequest, CreateLancamentoRequest request, LancamentosDbContext dbContext, ILogger<Program> logger) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                logger.LogWarning("Requisição de criação de lançamento sem X-Merchant-Id válido.");
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            try
            {
                var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
                var lancamento = Lancamento.Criar(
                    merchantId,
                    request.Tipo,
                    request.Valor,
                    request.DataLancamento,
                    request.Descricao,
                    request.Categoria,
                    hoje);

                dbContext.Lancamentos.Add(lancamento);
                dbContext.OutboxEvents.Add(lancamento.ToOutboxEvent());
                await dbContext.SaveChangesAsync();

                logger.LogInformation(
                    "Lançamento {LancamentoId} registrado para o merchant {MerchantId}: {Tipo} de {Valor} em {DataLancamento}.",
                    lancamento.Id, merchantId, lancamento.Tipo, lancamento.Valor, lancamento.DataLancamento);

                return Results.Created($"/api/v1/lancamentos/{lancamento.Id}", lancamento.ToResponse());
            }
            catch (DomainException ex)
            {
                logger.LogWarning(ex, "Falha de domínio ao registrar lançamento para o merchant {MerchantId}.", merchantId);
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        group.MapGet("/", async (HttpRequest httpRequest, DateOnly? de, DateOnly? ate, TipoLancamento? tipo, string? categoria, int? page, int? pageSize, LancamentosDbContext dbContext) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var currentPage = Math.Max(page ?? 1, 1);
            var currentPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

            var query = dbContext.Lancamentos.AsNoTracking().Where(x => x.MerchantId == merchantId);

            if (de.HasValue)
            {
                query = query.Where(x => x.DataLancamento >= de.Value);
            }

            if (ate.HasValue)
            {
                query = query.Where(x => x.DataLancamento <= ate.Value);
            }

            if (tipo.HasValue)
            {
                query = query.Where(x => x.Tipo == tipo.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(x => x.Categoria == categoria);
            }

            var items = await query
                .OrderByDescending(x => x.DataLancamento)
                .ThenByDescending(x => x.CriadoEm)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .Select(x => x.ToResponse())
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapGet("/{id:guid}", async (HttpRequest httpRequest, Guid id, LancamentosDbContext dbContext) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var lancamento = await dbContext.Lancamentos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId);

            return lancamento is null ? Results.NotFound() : Results.Ok(lancamento.ToResponse());
        });

        group.MapPost("/{id:guid}/estorno", async (HttpRequest httpRequest, Guid id, LancamentosDbContext dbContext, ILogger<Program> logger) =>
        {
            if (!MerchantContext.TryGetMerchantId(httpRequest, out var merchantId))
            {
                logger.LogWarning("Requisição de estorno sem X-Merchant-Id válido para o lançamento {LancamentoId}.", id);
                return Results.BadRequest(new { message = "Header X-Merchant-Id é obrigatório e deve ser um UUID válido." });
            }

            var original = await dbContext.Lancamentos.FirstOrDefaultAsync(x => x.Id == id && x.MerchantId == merchantId);
            if (original is null)
            {
                logger.LogWarning("Tentativa de estornar lançamento inexistente {LancamentoId} para o merchant {MerchantId}.", id, merchantId);
                return Results.NotFound();
            }

            var alreadyReversed = await dbContext.Lancamentos.AnyAsync(x => x.EstornoDoLancamentoId == id && x.MerchantId == merchantId);
            if (alreadyReversed)
            {
                logger.LogWarning("Tentativa de estornar lançamento {LancamentoId} já estornado para o merchant {MerchantId}.", id, merchantId);
                return Results.Conflict(new { message = "Lançamento já foi estornado." });
            }

            try
            {
                var estorno = original.Estornar(DateOnly.FromDateTime(DateTime.UtcNow));
                dbContext.Lancamentos.Add(estorno);
                dbContext.OutboxEvents.Add(estorno.ToOutboxEvent());
                await dbContext.SaveChangesAsync();

                logger.LogInformation(
                    "Estorno {EstornoId} registrado para o lançamento {LancamentoId} do merchant {MerchantId}.",
                    estorno.Id, id, merchantId);

                return Results.Created($"/api/v1/lancamentos/{estorno.Id}", estorno.ToResponse());
            }
            catch (DomainException ex)
            {
                logger.LogWarning(ex, "Falha de domínio ao estornar lançamento {LancamentoId} do merchant {MerchantId}.", id, merchantId);
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        return group;
    }
}
