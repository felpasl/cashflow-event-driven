import { parseResponse } from "@/lib/api-client";
import { buildHeaders, lancamentosApiUrl } from "@/lib/config";
import { Lancamento, NovoLancamentoInput } from "@/lib/types";

export async function listLancamentos(): Promise<Lancamento[]> {
  const response = await fetch(lancamentosApiUrl + "/api/v1/lancamentos?pageSize=50", {
    headers: buildHeaders()
  });
  return parseResponse<Lancamento[]>(response);
}

export async function criarLancamento(input: NovoLancamentoInput): Promise<Lancamento> {
  const response = await fetch(lancamentosApiUrl + "/api/v1/lancamentos", {
    method: "POST",
    headers: buildHeaders(),
    body: JSON.stringify(input)
  });
  return parseResponse<Lancamento>(response);
}

export async function estornarLancamento(id: string): Promise<Lancamento> {
  const response = await fetch(lancamentosApiUrl + "/api/v1/lancamentos/" + id + "/estorno", {
    method: "POST",
    headers: buildHeaders()
  });
  return parseResponse<Lancamento>(response);
}
