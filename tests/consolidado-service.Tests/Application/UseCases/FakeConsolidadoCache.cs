using ConsolidadoService.Application;
using ConsolidadoService.Contracts;

namespace ConsolidadoService.Tests.Application.UseCases;

internal sealed class FakeConsolidadoCache : IConsolidadoCache
{
    public ConsolidadoResponse? Cached { get; set; }

    public List<ConsolidadoResponse> Salvos { get; } = [];

    public List<(Guid MerchantId, DateOnly Data)> Invalidados { get; } = [];

    public Task<ConsolidadoResponse?> GetAsync(Guid merchantId, DateOnly data)
    {
        return Task.FromResult(Cached);
    }

    public Task SetAsync(ConsolidadoResponse response)
    {
        Salvos.Add(response);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid merchantId, DateOnly data)
    {
        Invalidados.Add((merchantId, data));
        return Task.CompletedTask;
    }
}
