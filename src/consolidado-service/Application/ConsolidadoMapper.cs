using ConsolidadoService.Contracts;
using ConsolidadoService.Domain;

namespace ConsolidadoService.Application;

public static class ConsolidadoMapper
{
    public static ConsolidadoResponse ToResponse(this SaldoDiario saldo)
    {
        return new ConsolidadoResponse(
            saldo.MerchantId,
            saldo.Data,
            saldo.TotalCreditos,
            saldo.TotalDebitos,
            saldo.SaldoDia,
            saldo.QuantidadeLancamentos,
            saldo.AtualizadoEm);
    }
}
