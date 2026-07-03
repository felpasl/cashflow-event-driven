export type TipoLancamento = "Credito" | "Debito";

export type Lancamento = {
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

export type Consolidado = {
  merchantId: string;
  data: string;
  totalCreditos: number;
  totalDebitos: number;
  saldoDia: number;
  quantidadeLancamentos: number;
  atualizadoEm: string;
};

export type NovoLancamentoInput = {
  tipo: TipoLancamento;
  valor: number;
  dataLancamento: string;
  descricao: string;
  categoria?: string | null;
};
