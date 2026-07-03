import { CircleAlert, RefreshCcw, RotateCcw } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { formatDate, money } from "@/lib/format";
import { Lancamento } from "@/lib/types";
import { cn } from "@/lib/utils";

export function LancamentosTable({
  lancamentos,
  loading,
  notice,
  error,
  onRefresh,
  onEstornar
}: {
  lancamentos: Lancamento[];
  loading: boolean;
  notice: string;
  error: string | null;
  onRefresh: () => void;
  onEstornar: (id: string) => void;
}) {
  return (
    <Card className="min-w-0">
      <CardHeader>
        <div className="min-w-0">
          <CardTitle>Lançamentos</CardTitle>
          <CardDescription>{lancamentos.length} registro(s) carregado(s)</CardDescription>
        </div>
        <CardAction>
          <Button variant="outline" disabled={loading} onClick={onRefresh} type="button">
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
                    onClick={() => onEstornar(item.id)}
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
  );
}
