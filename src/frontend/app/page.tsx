"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { CircleAlert, RefreshCcw, RotateCcw, Save, Scale, TrendingDown, TrendingUp } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

type TipoLancamento = "Credito" | "Debito";

type Lancamento = {
  id: string;
  merchantId: string;
  tipo: TipoLancamento;
  valor: number;
  dataLancamento: string;
  descricao: string;
  categoria?: string | null;
  estornoDoLancamentoId?: string | null;
  criadoEm: string;
};

type Consolidado = {
  merchantId: string;
  data: string;
  totalCreditos: number;
  totalDebitos: number;
  saldoDia: number;
  quantidadeLancamentos: number;
  atualizadoEm: string;
};

const lancamentosApiUrl = process.env.NEXT_PUBLIC_LANCAMENTOS_API_URL ?? "http://localhost:5001";
const consolidadoApiUrl = process.env.NEXT_PUBLIC_CONSOLIDADO_API_URL ?? "http://localhost:5002";
const merchantId = process.env.NEXT_PUBLIC_MERCHANT_ID ?? "00000000-0000-0000-0000-000000000001";

const money = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

function today() {
  return new Date().toISOString().slice(0, 10);
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("pt-BR", { timeZone: "UTC" }).format(new Date(value + "T00:00:00.000Z"));
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string } | null;
    throw new Error(body?.message ?? "Erro HTTP " + response.status);
  }

  return response.json() as Promise<T>;
}

export default function Home() {
  const [tipo, setTipo] = useState<TipoLancamento>("Credito");
  const [valor, setValor] = useState("150.75");
  const [dataLancamento, setDataLancamento] = useState(today());
  const [descricao, setDescricao] = useState("Venda cartão");
  const [categoria, setCategoria] = useState("Vendas");
  const [dataConsolidado, setDataConsolidado] = useState(today());
  const [lancamentos, setLancamentos] = useState<Lancamento[]>([]);
  const [consolidado, setConsolidado] = useState<Consolidado | null>(null);
  const [loading, setLoading] = useState(false);
  const [notice, setNotice] = useState("Pronto para registrar lançamentos.");
  const [error, setError] = useState<string | null>(null);

  const headers = useMemo(() => ({
    "Content-Type": "application/json",
    "X-Merchant-Id": merchantId
  }), []);

  async function loadLancamentos() {
    const data = await fetch(lancamentosApiUrl + "/api/v1/lancamentos?pageSize=50", { headers })
      .then(parseResponse<Lancamento[]>);
    setLancamentos(data);
  }

  async function loadConsolidado(date = dataConsolidado) {
    const data = await fetch(consolidadoApiUrl + "/api/v1/consolidados/" + date, { headers })
      .then(parseResponse<Consolidado>);
    setConsolidado(data);
  }

  async function refreshAll(date = dataConsolidado) {
    setError(null);
    await Promise.all([loadLancamentos(), loadConsolidado(date)]);
  }

  useEffect(() => {
    refreshAll().catch((err: Error) => setError(err.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await fetch(lancamentosApiUrl + "/api/v1/lancamentos", {
        method: "POST",
        headers,
        body: JSON.stringify({
          tipo,
          valor: Number(valor),
          dataLancamento,
          descricao,
          categoria: categoria || null
        })
      }).then(parseResponse<Lancamento>);

      setNotice("Lançamento registrado. O consolidado será atualizado de forma assíncrona.");
      await refreshAll(dataLancamento);
      setDataConsolidado(dataLancamento);
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
      await fetch(lancamentosApiUrl + "/api/v1/lancamentos/" + id + "/estorno", {
        method: "POST",
        headers
      }).then(parseResponse<Lancamento>);

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

  const saldo = consolidado?.saldoDia ?? 0;

  return (
    <main className="min-h-screen bg-muted/30 px-4 py-6 text-foreground sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-5">
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

        <section className="grid gap-5 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.35fr)]">
          <div className="flex flex-col gap-5">
            <Card>
              <CardHeader>
                <CardTitle>Novo lançamento</CardTitle>
                <CardDescription>Registre créditos, débitos e categorias para projeção diária.</CardDescription>
              </CardHeader>
              <CardContent>
                <form className="grid gap-4" onSubmit={submit}>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="grid gap-2">
                      <Label htmlFor="tipo">Tipo</Label>
                      <Select value={tipo} onValueChange={(value) => setTipo(value as TipoLancamento)}>
                        <SelectTrigger id="tipo" className="w-full">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Credito">Crédito</SelectItem>
                          <SelectItem value="Debito">Débito</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="grid gap-2">
                      <Label htmlFor="valor">Valor</Label>
                      <Input id="valor" value={valor} onChange={(event) => setValor(event.target.value)} inputMode="decimal" />
                    </div>
                  </div>

                  <div className="grid gap-2">
                    <Label htmlFor="data">Data</Label>
                    <Input id="data" type="date" value={dataLancamento} onChange={(event) => setDataLancamento(event.target.value)} />
                  </div>

                  <div className="grid gap-2">
                    <Label htmlFor="descricao">Descrição</Label>
                    <Input id="descricao" value={descricao} onChange={(event) => setDescricao(event.target.value)} />
                  </div>

                  <div className="grid gap-2">
                    <Label htmlFor="categoria">Categoria</Label>
                    <Input id="categoria" value={categoria} onChange={(event) => setCategoria(event.target.value)} />
                  </div>

                  <Button disabled={loading} type="submit" className="w-full sm:w-fit">
                    <Save /> Registrar
                  </Button>
                </form>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <div>
                  <CardTitle>Consolidado diário</CardTitle>
                  <CardDescription>Resumo financeiro da data selecionada.</CardDescription>
                </div>
                <CardAction>
                  <Button variant="outline" disabled={loading} onClick={refreshConsolidado} type="button">
                    <RefreshCcw /> Atualizar
                  </Button>
                </CardAction>
              </CardHeader>
              <CardContent className="grid gap-4">
                <div className="grid gap-2">
                  <Label htmlFor="dataConsolidado">Data</Label>
                  <Input id="dataConsolidado" type="date" value={dataConsolidado} onChange={(event) => setDataConsolidado(event.target.value)} />
                </div>

                <div className="grid gap-3 sm:grid-cols-3">
                  <MetricCard icon={TrendingUp} label="Créditos" value={money.format(consolidado?.totalCreditos ?? 0)} tone="positive" />
                  <MetricCard icon={TrendingDown} label="Débitos" value={money.format(consolidado?.totalDebitos ?? 0)} tone="negative" />
                  <MetricCard icon={Scale} label="Saldo" value={money.format(saldo)} tone={saldo < 0 ? "negative" : "positive"} />
                </div>
              </CardContent>
            </Card>
          </div>

          <Card className="min-w-0">
            <CardHeader>
              <div className="min-w-0">
                <CardTitle>Lançamentos</CardTitle>
                <CardDescription>{lancamentos.length} registro(s) carregado(s)</CardDescription>
              </div>
              <CardAction>
                <Button variant="outline" disabled={loading} onClick={() => refreshAll()} type="button">
                  <RefreshCcw /> Recarregar
                </Button>
              </CardAction>
            </CardHeader>
            <CardContent className="grid gap-4">
              <Alert variant={error ? "destructive" : "default"}>
                <CircleAlert />
                <AlertTitle>{error ? "Não foi possível atualizar" : "Status"}</AlertTitle>
                <AlertDescription>{error ?? notice}</AlertDescription>
              </Alert>

              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data</TableHead>
                    <TableHead>Tipo</TableHead>
                    <TableHead>Descrição</TableHead>
                    <TableHead>Categoria</TableHead>
                    <TableHead className="text-right">Valor</TableHead>
                    <TableHead className="text-right">Ação</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {lancamentos.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} className="h-24 text-center text-muted-foreground">
                        Nenhum lançamento encontrado.
                      </TableCell>
                    </TableRow>
                  ) : lancamentos.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell>{formatDate(item.dataLancamento)}</TableCell>
                      <TableCell>
                        <Badge variant={item.tipo === "Credito" ? "default" : "destructive"}>{item.tipo}</Badge>
                      </TableCell>
                      <TableCell className="max-w-56 truncate font-medium">{item.descricao}</TableCell>
                      <TableCell className="text-muted-foreground">{item.categoria ?? "-"}</TableCell>
                      <TableCell className={cn("text-right font-medium", item.tipo === "Credito" ? "text-emerald-700" : "text-red-700")}>
                        {money.format(item.valor)}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="destructive"
                          size="sm"
                          disabled={loading || Boolean(item.estornoDoLancamentoId)}
                          onClick={() => estornar(item.id)}
                          type="button"
                          title="Estornar lançamento"
                        >
                          <RotateCcw /> Estornar
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </section>
      </div>
    </main>
  );
}

function MetricCard({
  icon: Icon,
  label,
  value,
  tone
}: {
  icon: typeof TrendingUp;
  label: string;
  value: string;
  tone: "positive" | "negative";
}) {
  return (
    <Card size="sm" className="rounded-lg">
      <CardContent className="flex items-center gap-3 py-1">
        <span className={cn(
          "flex size-9 items-center justify-center rounded-lg",
          tone === "positive" ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"
        )}>
          <Icon className="size-4" />
        </span>
        <div className="min-w-0">
          <p className="text-xs text-muted-foreground">{label}</p>
          <p className={cn("truncate text-base font-semibold", tone === "positive" ? "text-emerald-700" : "text-red-700")}>{value}</p>
        </div>
      </CardContent>
    </Card>
  );
}
