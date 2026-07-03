export const lancamentosApiUrl = process.env.NEXT_PUBLIC_LANCAMENTOS_API_URL ?? "http://localhost:5001";
export const consolidadoApiUrl = process.env.NEXT_PUBLIC_CONSOLIDADO_API_URL ?? "http://localhost:5002";
export const merchantId = process.env.NEXT_PUBLIC_MERCHANT_ID ?? "00000000-0000-0000-0000-000000000001";

export function buildHeaders(): HeadersInit {
  return {
    "Content-Type": "application/json",
    "X-Merchant-Id": merchantId
  };
}
