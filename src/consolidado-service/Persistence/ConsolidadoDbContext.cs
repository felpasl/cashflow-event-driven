using ConsolidadoService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoService.Persistence;

public sealed class ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : DbContext(options)
{
    public DbSet<SaldoDiario> SaldosDiarios => Set<SaldoDiario>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaldoDiario>(entity =>
        {
            entity.ToTable("saldos_diarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MerchantId).HasColumnName("merchant_id").IsRequired();
            entity.Property(x => x.Data).HasColumnName("data").IsRequired();
            entity.Property(x => x.TotalCreditos).HasColumnName("total_creditos").HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.TotalDebitos).HasColumnName("total_debitos").HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.SaldoDia).HasColumnName("saldo_dia").HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.QuantidadeLancamentos).HasColumnName("quantidade_lancamentos").IsRequired();
            entity.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();
            entity.HasIndex(x => new { x.MerchantId, x.Data }).IsUnique();
        });

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.ToTable("processed_events");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventId).HasColumnName("event_id");
            entity.Property(x => x.StreamMessageId).HasColumnName("stream_message_id").HasMaxLength(100).IsRequired();
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProcessedAt).HasColumnName("processed_at").IsRequired();
        });
    }
}
