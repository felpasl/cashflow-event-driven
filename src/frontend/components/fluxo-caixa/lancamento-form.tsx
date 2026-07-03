import { FormEvent, useState } from "react";
import { Save } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { today } from "@/lib/format";
import { NovoLancamentoInput, TipoLancamento } from "@/lib/types";

export function LancamentoForm({
  loading,
  onSubmit
}: {
  loading: boolean;
  onSubmit: (input: NovoLancamentoInput) => void | Promise<void>;
}) {
  const [tipo, setTipo] = useState<TipoLancamento>("Credito");
  const [valor, setValor] = useState("150.75");
  const [dataLancamento, setDataLancamento] = useState(today());
  const [descricao, setDescricao] = useState("Venda cartão");
  const [categoria, setCategoria] = useState("Vendas");

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({
      tipo,
      valor: Number(valor),
      dataLancamento,
      descricao,
      categoria: categoria || null
    });
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Novo lançamento</CardTitle>
        <CardDescription>Registre créditos, débitos e categorias para projeção diária.</CardDescription>
      </CardHeader>
      <CardContent>
        <form className="grid gap-4" onSubmit={handleSubmit}>
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
  );
}
