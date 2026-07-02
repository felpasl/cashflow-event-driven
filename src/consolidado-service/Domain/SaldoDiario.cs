namespace ConsolidadoService.Domain;

public sealed class SaldoDiario
{
    private SaldoDiario()
    {
    }

    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public DateOnly Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal SaldoDia { get; private set; }
    public int QuantidadeLancamentos { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public static SaldoDiario Create(Guid merchantId, DateOnly data)
    {
        return new SaldoDiario
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Data = data,
            AtualizadoEm = DateTime.UtcNow
        };
    }

    public void Apply(TipoLancamento tipo, decimal valor)
    {
        if (valor <= 0)
        {
            throw new InvalidOperationException("Valor do evento deve ser maior que zero.");
        }

        if (tipo == TipoLancamento.Credito)
        {
            TotalCreditos += valor;
        }
        else
        {
            TotalDebitos += valor;
        }

        SaldoDia = TotalCreditos - TotalDebitos;
        QuantidadeLancamentos++;
        AtualizadoEm = DateTime.UtcNow;
    }
}
