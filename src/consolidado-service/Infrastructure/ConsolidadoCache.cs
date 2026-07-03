using System.Text.Json;
using ConsolidadoService.Application;
using ConsolidadoService.Contracts;
using StackExchange.Redis;

namespace ConsolidadoService.Infrastructure;

public sealed class ConsolidadoCache(IConnectionMultiplexer redis, IConfiguration configuration) : IConsolidadoCache
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(configuration.GetValue("Redis:CacheTtlSeconds", 60));

    public async Task<ConsolidadoResponse?> GetAsync(Guid merchantId, DateOnly data)
    {
        var value = await _database.StringGetAsync(Key(merchantId, data));
        return value.HasValue ? JsonSerializer.Deserialize<ConsolidadoResponse>(value.ToString()) : null;
    }

    public Task SetAsync(ConsolidadoResponse response)
    {
        return _database.StringSetAsync(
            Key(response.MerchantId, response.Data),
            JsonSerializer.Serialize(response),
            _ttl);
    }

    public Task InvalidateAsync(Guid merchantId, DateOnly data)
    {
        return _database.KeyDeleteAsync(Key(merchantId, data));
    }

    private static string Key(Guid merchantId, DateOnly data) => $"consolidado:{merchantId}:{data:yyyy-MM-dd}";
}
