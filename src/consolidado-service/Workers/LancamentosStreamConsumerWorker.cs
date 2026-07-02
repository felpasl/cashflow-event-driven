using System.Text.Json;
using ConsolidadoService.Contracts;
using ConsolidadoService.Domain;
using ConsolidadoService.Infrastructure;
using ConsolidadoService.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ConsolidadoService.Workers;

public sealed class LancamentosStreamConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IConnectionMultiplexer redis,
    ILogger<LancamentosStreamConsumerWorker> logger,
    IConfiguration configuration) : BackgroundService
{
    private readonly string _streamName = configuration.GetValue("Redis:StreamName", "cashflow.lancamentos")!;
    private readonly string _groupName = configuration.GetValue("Redis:ConsumerGroup", "consolidado-service")!;
    private readonly string _consumerName = $"consumer-{Environment.MachineName}-{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = redis.GetDatabase();
        await EnsureGroupAsync(database);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await database.StreamReadGroupAsync(
                    _streamName,
                    _groupName,
                    _consumerName,
                    ">",
                    count: 10);

                if (entries.Length == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    await ProcessEntryAsync(database, entry, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao consumir Redis Streams.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task EnsureGroupAsync(IDatabase database)
    {
        try
        {
            await database.StreamCreateConsumerGroupAsync(_streamName, _groupName, "0-0", createStream: true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
        {
            // Consumer group already exists.
        }
    }

    private async Task ProcessEntryAsync(IDatabase database, StreamEntry entry, CancellationToken cancellationToken)
    {
        var values = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        var eventId = Guid.Parse(values["eventId"]);
        var eventType = values["eventType"];
        var payloadJson = values["payload"];

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidadoDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<ConsolidadoCache>();

        if (await dbContext.ProcessedEvents.AnyAsync(x => x.EventId == eventId, cancellationToken))
        {
            logger.LogInformation("Evento {EventId} ({EventType}) já processado, ignorando.", eventId, eventType);
            await database.StreamAcknowledgeAsync(_streamName, _groupName, entry.Id);
            return;
        }

        var payload = JsonSerializer.Deserialize<LancamentoRegistradoPayload>(payloadJson)
            ?? throw new InvalidOperationException("Payload do evento inválido.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var saldo = await dbContext.SaldosDiarios
            .FirstOrDefaultAsync(x => x.MerchantId == payload.MerchantId && x.Data == payload.DataLancamento, cancellationToken);

        if (saldo is null)
        {
            saldo = SaldoDiario.Create(payload.MerchantId, payload.DataLancamento);
            dbContext.SaldosDiarios.Add(saldo);
        }

        saldo.Apply(payload.Tipo, payload.Valor);
        dbContext.ProcessedEvents.Add(ProcessedEvent.Create(eventId, entry.Id!, eventType));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await cache.InvalidateAsync(payload.MerchantId, payload.DataLancamento);
        await database.StreamAcknowledgeAsync(_streamName, _groupName, entry.Id);

        logger.LogInformation(
            "Evento {EventId} ({EventType}) aplicado: saldo do merchant {MerchantId} atualizado para o dia {Data}.",
            eventId, eventType, payload.MerchantId, payload.DataLancamento);
    }
}
