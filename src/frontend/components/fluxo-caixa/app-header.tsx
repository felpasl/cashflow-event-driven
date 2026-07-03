import { merchantId } from "@/lib/config";

export function AppHeader() {
  return (
    <header className="flex flex-col gap-4 rounded-xl border bg-card px-4 py-4 shadow-sm sm:flex-row sm:items-center sm:justify-between sm:px-5">
      <div className="flex items-center gap-3">
        <div className="relative h-8 w-12 shrink-0" aria-hidden="true">
          <span className="absolute left-0 top-0 h-8 w-8 rounded-full bg-[#eb001b] opacity-95" />
          <span className="absolute right-0 top-0 h-8 w-8 rounded-full bg-[#ff5f00] opacity-95 mix-blend-multiply" />
        </div>
        <div>
          <h1 className="text-xl font-semibold tracking-normal sm:text-2xl">Fluxo de caixa</h1>
          <p className="text-sm text-muted-foreground">Lançamentos e saldo diário consolidado</p>
        </div>
      </div>
      <div className="max-w-full text-left text-xs text-muted-foreground sm:text-right">
        <span className="font-medium text-foreground">Merchant</span>
        <p className="break-all">{merchantId}</p>
      </div>
    </header>
  );
}
