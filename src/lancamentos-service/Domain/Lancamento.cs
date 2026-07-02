namespace LancamentosService.Domain;

public sealed class Lancamento
{
    private Lancamento()
    {
        Descricao = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public string? Categoria { get; private set; }
    public DateOnly DataLancamento { get; private set; }
    public Guid? EstornoDoLancamentoId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public static Lancamento Criar(
        Guid merchantId,
        TipoLancamento tipo,
        decimal valor,
        DateOnly dataLancamento,
        string descricao,
        string? categoria,
        DateOnly hoje)
    {
        if (merchantId == Guid.Empty)
        {
            throw new DomainException("MerchantId é obrigatório.");
        }

        if (valor <= 0)
        {
            throw new DomainException("Valor deve ser maior que zero.");
        }

        if (dataLancamento > hoje)
        {
            throw new DomainException("Data do lançamento não pode ser futura.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new DomainException("Descrição é obrigatória.");
        }

        return new Lancamento
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Tipo = tipo,
            Valor = decimal.Round(valor, 2),
            DataLancamento = dataLancamento,
            Descricao = descricao.Trim(),
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim(),
            CriadoEm = DateTime.UtcNow
        };
    }

    public Lancamento Estornar(DateOnly hoje)
    {
        if (EstornoDoLancamentoId.HasValue)
        {
            throw new DomainException("Lançamento de estorno não pode ser estornado novamente.");
        }

        return new Lancamento
        {
            Id = Guid.NewGuid(),
            MerchantId = MerchantId,
            Tipo = Tipo == TipoLancamento.Credito ? TipoLancamento.Debito : TipoLancamento.Credito,
            Valor = Valor,
            DataLancamento = hoje,
            Descricao = $"Estorno de {Descricao}",
            Categoria = Categoria,
            EstornoDoLancamentoId = Id,
            CriadoEm = DateTime.UtcNow
        };
    }
}
