namespace ConsolidadoService.Domain;

public sealed class ProcessedEvent
{
    private ProcessedEvent()
    {
        StreamMessageId = string.Empty;
        EventType = string.Empty;
    }

    public Guid EventId { get; private set; }
    public string StreamMessageId { get; private set; }
    public string EventType { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    public static ProcessedEvent Create(Guid eventId, string streamMessageId, string eventType)
    {
        return new ProcessedEvent
        {
            EventId = eventId,
            StreamMessageId = streamMessageId,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
