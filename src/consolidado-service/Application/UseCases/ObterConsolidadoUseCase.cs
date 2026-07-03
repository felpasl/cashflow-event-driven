using ConsolidadoService.Contracts;

namespace ConsolidadoService.Application.UseCases;

public sealed class ObterConsolidadoUseCase(IConsolidadoRepository repository, IConsolidadoCache cache)
{
    public async Task<ConsolidadoResponse> ExecutarAsync(Guid merchantId, DateOnly data)
    {
        var cached = await cache.GetAsync(merchantId, data);
        if (cached is not null)
        {
            return cached;
        }

        var saldo = await repository.ObterSaldoAsync(merchantId, data);
        var response = saldo?.ToResponse() ?? new ConsolidadoResponse(merchantId, data, 0, 0, 0, 0, DateTime.UtcNow);

        await cache.SetAsync(response);

        return response;
    }
}
