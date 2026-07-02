namespace LancamentosService.Domain;

public sealed class OutboxEvent
{
    private OutboxEvent()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public int EventVersion { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Payload { get; private set; }
    public OutboxStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    public static OutboxEvent Create(string eventType, int eventVersion, string payload)
    {
        return new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EventVersion = eventVersion,
            OccurredAt = DateTime.UtcNow,
            Payload = payload,
            Status = OutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkSent()
    {
        Status = OutboxStatus.Sent;
        SentAt = DateTime.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length > 1000 ? error[..1000] : error;
        Status = Attempts >= 10 ? OutboxStatus.Failed : OutboxStatus.Pending;
    }
}

public enum OutboxStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}
