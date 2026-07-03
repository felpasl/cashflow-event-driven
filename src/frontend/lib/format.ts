export const money = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export function today() {
  return new Date().toISOString().slice(0, 10);
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat("pt-BR", { timeZone: "UTC" }).format(new Date(value + "T00:00:00.000Z"));
}
