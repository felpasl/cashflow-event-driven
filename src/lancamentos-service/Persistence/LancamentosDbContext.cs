using LancamentosService.Domain;
using Microsoft.EntityFrameworkCore;

namespace LancamentosService.Persistence;

public sealed class LancamentosDbContext(DbContextOptions<LancamentosDbContext> options) : DbContext(options)
{
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.ToTable("lancamentos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MerchantId).HasColumnName("merchant_id").IsRequired();
            entity.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Valor).HasColumnName("valor").HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(300).IsRequired();
            entity.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(100);
            entity.Property(x => x.DataLancamento).HasColumnName("data_lancamento").IsRequired();
            entity.Property(x => x.EstornoDoLancamentoId).HasColumnName("estorno_do_lancamento_id");
            entity.Property(x => x.CriadoEm).HasColumnName("criado_em").IsRequired();
            entity.HasIndex(x => new { x.MerchantId, x.DataLancamento });
        });

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.ToTable("outbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(x => x.EventVersion).HasColumnName("event_version").IsRequired();
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
            entity.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Attempts).HasColumnName("attempts").IsRequired();
            entity.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.SentAt).HasColumnName("sent_at");
            entity.HasIndex(x => new { x.Status, x.CreatedAt });
        });
    }
}
