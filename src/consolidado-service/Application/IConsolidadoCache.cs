using ConsolidadoService.Contracts;

namespace ConsolidadoService.Application;

public interface IConsolidadoCache
{
    Task<ConsolidadoResponse?> GetAsync(Guid merchantId, DateOnly data);

    Task SetAsync(ConsolidadoResponse response);

    Task InvalidateAsync(Guid merchantId, DateOnly data);
}
