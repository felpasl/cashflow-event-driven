namespace ConsolidadoService.Contracts;

public sealed record ConsolidadoResponse(
    Guid MerchantId,
    DateOnly Data,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal SaldoDia,
    int QuantidadeLancamentos,
    DateTime AtualizadoEm);
