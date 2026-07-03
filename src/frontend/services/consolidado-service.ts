import { parseResponse } from "@/lib/api-client";
import { buildHeaders, consolidadoApiUrl } from "@/lib/config";
import { Consolidado } from "@/lib/types";

export async function getConsolidado(data: string): Promise<Consolidado> {
  const response = await fetch(consolidadoApiUrl + "/api/v1/consolidados/" + data, {
    headers: buildHeaders()
  });
  return parseResponse<Consolidado>(response);
}
