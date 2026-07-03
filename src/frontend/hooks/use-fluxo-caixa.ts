import { useEffect, useState } from "react";

import { today } from "@/lib/format";
import { Consolidado, Lancamento, NovoLancamentoInput } from "@/lib/types";
import { getConsolidado } from "@/services/consolidado-service";
import { criarLancamento, estornarLancamento, listLancamentos } from "@/services/lancamentos-service";

export function useFluxoCaixa() {
  const [dataConsolidado, setDataConsolidado] = useState(today());
  const [lancamentos, setLancamentos] = useState<Lancamento[]>([]);
  const [consolidado, setConsolidado] = useState<Consolidado | null>(null);
  const [loading, setLoading] = useState(false);
  const [notice, setNotice] = useState("Pronto para registrar lançamentos.");
  const [error, setError] = useState<string | null>(null);

  async function loadConsolidado(date = dataConsolidado) {
    setConsolidado(await getConsolidado(date));
  }

  async function refreshAll(date = dataConsolidado) {
    setError(null);
    await Promise.all([listLancamentos().then(setLancamentos), loadConsolidado(date)]);
  }

  useEffect(() => {
    refreshAll().catch((err: Error) => setError(err.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function registrarLancamento(input: NovoLancamentoInput) {
    setLoading(true);
    setError(null);

    try {
      await criarLancamento(input);
      setNotice("Lançamento registrado. O consolidado será atualizado de forma assíncrona.");
      await refreshAll(input.dataLancamento);
      setDataConsolidado(input.dataLancamento);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro inesperado.");
    } finally {
      setLoading(false);
    }
  }

  async function estornar(id: string) {
    setLoading(true);
    setError(null);

    try {
      await estornarLancamento(id);
      setNotice("Estorno registrado. Aguarde a projeção atualizar o saldo diário.");
      await refreshAll();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro inesperado.");
    } finally {
      setLoading(false);
    }
  }

  async function refreshConsolidado() {
    setLoading(true);
    setError(null);

    try {
      await loadConsolidado();
      setNotice("Consolidado atualizado a partir da projeção disponível.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro inesperado.");
    } finally {
      setLoading(false);
    }
  }

  async function alterarDataConsolidado(date: string) {
    setDataConsolidado(date);
    setLoading(true);
    setError(null);

    try {
      await loadConsolidado(date);
      setNotice("Consolidado atualizado para a data selecionada.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro inesperado.");
    } finally {
      setLoading(false);
    }
  }

  return {
    dataConsolidado,
    setDataConsolidado,
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
  };
}
