import { RefreshCcw, Scale, TrendingDown, TrendingUp } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { money } from "@/lib/format";
import { Consolidado } from "@/lib/types";

import { MetricCard } from "./metric-card";

export function ConsolidadoCard({
  consolidado,
  dataConsolidado,
  loading,
  onDataChange,
  onRefresh
}: {
  consolidado: Consolidado | null;
  dataConsolidado: string;
  loading: boolean;
  onDataChange: (data: string) => void;
  onRefresh: () => void;
}) {
  const saldo = consolidado?.saldoDia ?? 0;

  return (
    <Card>
      <CardHeader>
        <div>
          <CardTitle>Consolidado diário</CardTitle>
          <CardDescription>Resumo financeiro da data selecionada.</CardDescription>
        </div>
        <CardAction>
          <Button variant="outline" disabled={loading} onClick={onRefresh} type="button">
            <RefreshCcw /> Atualizar
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent className="grid gap-4">
        <div className="grid gap-2">
          <Label htmlFor="dataConsolidado">Data</Label>
          <Input
            id="dataConsolidado"
            type="date"
            value={dataConsolidado}
            onChange={(event) => onDataChange(event.target.value)}
          />
        </div>

        <div className="grid gap-3 sm:grid-cols-3">
          <MetricCard icon={TrendingUp} label="Créditos" value={money.format(consolidado?.totalCreditos ?? 0)} tone="positive" />
          <MetricCard icon={TrendingDown} label="Débitos" value={money.format(consolidado?.totalDebitos ?? 0)} tone="negative" />
          <MetricCard icon={Scale} label="Saldo" value={money.format(saldo)} tone={saldo < 0 ? "negative" : "positive"} />
        </div>
      </CardContent>
    </Card>
  );
}
