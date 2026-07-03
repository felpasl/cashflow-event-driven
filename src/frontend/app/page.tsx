"use client";

import { AppHeader } from "@/components/fluxo-caixa/app-header";
import { ConsolidadoCard } from "@/components/fluxo-caixa/consolidado-card";
import { LancamentoForm } from "@/components/fluxo-caixa/lancamento-form";
import { LancamentosTable } from "@/components/fluxo-caixa/lancamentos-table";
import { useFluxoCaixa } from "@/hooks/use-fluxo-caixa";

export default function Home() {
  const {
    dataConsolidado,
    alterarDataConsolidado,
    lancamentos,
    consolidado,
    loading,
    notice,
    error,
    registrarLancamento,
    estornar,
    refreshConsolidado,
    refreshAll
  } = useFluxoCaixa();

  return (
    <main className="min-h-screen bg-muted/30 px-4 py-6 text-foreground sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-5">
        <AppHeader />

        <section className="grid gap-5 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.35fr)]">
          <div className="flex flex-col gap-5">
            <LancamentoForm loading={loading} onSubmit={registrarLancamento} />
            <ConsolidadoCard
              consolidado={consolidado}
              dataConsolidado={dataConsolidado}
              loading={loading}
              onDataChange={alterarDataConsolidado}
              onRefresh={refreshConsolidado}
            />
          </div>

          <LancamentosTable
            lancamentos={lancamentos}
            loading={loading}
            notice={notice}
            error={error}
            onRefresh={() => refreshAll()}
            onEstornar={estornar}
          />
        </section>
      </div>
    </main>
  );
}
