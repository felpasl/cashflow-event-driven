using LancamentosService.Domain;
using LancamentosService.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace LancamentosService.Workers;

public sealed class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IConnectionMultiplexer redis,
    ILogger<OutboxPublisherWorker> logger,
    IConfiguration configuration) : BackgroundService
{
    private readonly string _streamName = configuration.GetValue("Redis:StreamName", "cashflow.lancamentos")!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao publicar eventos da outbox.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task PublishPendingEventsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();
        var database = redis.GetDatabase();

        var events = await dbContext.OutboxEvents
            .Where(x => x.Status == OutboxStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            return;
        }
        else
        {
            logger.LogInformation("Publicando {Count} eventos pendentes da outbox no Redis Streams {StreamName}.", events.Count, _streamName);
        }

        foreach (var outboxEvent in events)
        {
            try
            {
                await database.StreamAddAsync(_streamName, new NameValueEntry[]
                {
                    new("eventId", outboxEvent.Id.ToString()),
                    new("eventType", outboxEvent.EventType),
                    new("eventVersion", outboxEvent.EventVersion.ToString()),
                    new("occurredAt", outboxEvent.OccurredAt.ToString("O")),
                    new("payload", outboxEvent.Payload)
                });

                outboxEvent.MarkSent();
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Evento {EventId} ({EventType}) publicado no Redis Streams {StreamName}.",
                    outboxEvent.Id, outboxEvent.EventType, _streamName);
            }
            catch (Exception ex)
            {
                outboxEvent.MarkFailed(ex.Message);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogError(ex, "Falha ao publicar evento {EventId} no Redis Streams.", outboxEvent.Id);
            }
        }
    }
}
