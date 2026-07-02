# Plano de Implementação — Projetos em `src`

Este documento consolida as decisões de implementação do MVP para a solução de controle de fluxo de caixa. Ele complementa a visão arquitetural descrita em [02-solucao-arquitetural.md](02-solucao-arquitetural.md), trazendo a divisão prática dos projetos, responsabilidades e interfaces.

## 1. Estrutura proposta

```text
src/
  frontend/
  lancamentos-service/
  consolidado-service/

tests/
  lancamentos-service.Tests/
  consolidado-service.Tests/

scripts/
  start.sh
  stop.sh
  reset.sh

docker-compose.yml
```

O MVP terá três projetos executáveis:

- `frontend`: interface web para o comerciante.
- `lancamentos-service`: serviço responsável pelo ledger e pela publicação de eventos.
- `consolidado-service`: serviço responsável pela projeção de saldo diário.

Não haverá API Gateway/BFF executável no MVP. O gateway permanece como componente de arquitetura alvo documentado, mas a interface web chamará diretamente os dois serviços.

## 2. Stack de implementação

| Componente | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs |
| Persistência | Entity Framework Core + PostgreSQL |
| Mensageria | Redis Streams |
| Cache | Redis |
| Frontend | React + Next.js + TypeScript + shadcn/Base UI |
| Testes | Unitários no backend |
| Execução local standalone | `docker-compose.yml` na raiz |

O backend seguirá Clean Architecture dentro de um único `.csproj` por serviço. Class libraries adicionais só devem ser criadas se houver necessidade real de compartilhar contratos técnicos estáveis entre serviços.

## 3. Organização interna dos backends

Cada serviço backend deve manter a separação por pastas:

```text
Domain/
Application/
Infrastructure/
Persistence/
Endpoints/
Workers/
Contracts/
```

Responsabilidades esperadas:

- `Domain`: entidades, value objects e regras puras do domínio.
- `Application`: casos de uso e orquestração.
- `Persistence`: `DbContext`, mapeamentos EF e migrations.
- `Infrastructure`: integrações externas, Redis, cache, publicação/consumo.
- `Endpoints`: adaptação HTTP via Minimal APIs.
- `Workers`: jobs em background.
- `Contracts`: requests/responses HTTP e mensagens técnicas do serviço.

Domínio não deve ser compartilhado entre serviços. Se for necessário compartilhar algo, a opção preferencial é uma class library pequena em `src/shared/Cashflow.Contracts`, contendo apenas contratos neutros como eventos versionados. Entidades como `Lancamento` e `SaldoDiario` não devem ser compartilhadas.

## 4. `frontend`

### Responsabilidades

- Expor uma UI operacional simples para o comerciante.
- Cadastrar lançamentos de débito e crédito.
- Listar lançamentos.
- Estornar lançamentos.
- Consultar consolidado diário.
- Consultar consolidado por período.
- Enviar `X-Merchant-Id` em todas as chamadas de negócio.
- Indicar ao usuário que o consolidado é atualizado de forma eventual.

### Não responsabilidades

- Calcular saldo.
- Aplicar regra financeira.
- Persistir dados de negócio.
- Conhecer detalhes da mensageria.

### Configuração

O frontend deve usar variáveis de ambiente:

```text
NEXT_PUBLIC_LANCAMENTOS_API_URL=http://localhost:5001
NEXT_PUBLIC_CONSOLIDADO_API_URL=http://localhost:5002
NEXT_PUBLIC_MERCHANT_ID=00000000-0000-0000-0000-000000000001
```

## 5. `lancamentos-service`

### Responsabilidades

- Validar e registrar lançamentos financeiros.
- Manter o ledger append-only como fonte da verdade.
- Permitir lançamentos retroativos.
- Bloquear lançamentos com data futura.
- Estornar lançamento por meio de um novo lançamento oposto.
- Persistir `lancamentos` e `outbox_events` na mesma transação.
- Publicar eventos no Redis Streams por meio de worker interno.
- Expor consultas do ledger.

### Não responsabilidades

- Calcular consolidado diário.
- Consultar o banco do `consolidado-service`.
- Fazer chamada síncrona para o `consolidado-service`.
- Depender da disponibilidade do consumidor de eventos.

### Modelo de dados principal

Database: `lancamentos_db`.

Tabela `lancamentos`:

| Campo | Observação |
|---|---|
| `id` | UUID |
| `merchant_id` | UUID obrigatório |
| `tipo` | `Credito` ou `Debito` |
| `valor` | `numeric(18,2)`, maior que zero |
| `descricao` | Texto livre |
| `categoria` | Opcional |
| `data_lancamento` | Data do lançamento; não pode ser futura |
| `estorno_do_lancamento_id` | Referência opcional ao lançamento original |
| `criado_em` | Timestamp UTC |

Tabela `outbox_events`:

| Campo | Observação |
|---|---|
| `id` | UUID do evento |
| `event_type` | Ex: `LancamentoRegistrado` |
| `event_version` | Ex: `1` |
| `occurred_at` | Timestamp UTC |
| `payload` | JSON versionado |
| `status` | `Pending`, `Sent`, `Failed` |
| `attempts` | Contador de tentativas |
| `last_error` | Último erro, se houver |
| `created_at` | Timestamp UTC |
| `sent_at` | Timestamp UTC opcional |

O outbox é separado como tabela técnica, mas fica no mesmo database do `lancamentos-service` para preservar a atomicidade transacional entre salvar o lançamento e registrar o evento pendente.

### Endpoints

Todos os endpoints de negócio exigem header `X-Merchant-Id` com UUID válido. Health checks não exigem esse header.

```text
POST   /api/v1/lancamentos
GET    /api/v1/lancamentos
GET    /api/v1/lancamentos/{id}
POST   /api/v1/lancamentos/{id}/estorno
GET    /health/live
GET    /health/ready
```

Exemplo de request:

```json
{
  "tipo": "Credito",
  "valor": 150.75,
  "dataLancamento": "2026-07-02",
  "descricao": "Venda cartão",
  "categoria": "Vendas"
}
```

Regras:

- `valor` deve ser maior que zero.
- `tipo` define o sentido financeiro; não usar valor negativo.
- `dataLancamento` pode ser retroativa.
- `dataLancamento` futura deve retornar `400 Bad Request`.
- Estorno cria um novo lançamento com tipo oposto, mesmo valor e `estornoDoLancamentoId`.

## 6. `consolidado-service`

### Responsabilidades

- Consumir eventos de lançamento via Redis Streams.
- Garantir idempotência por `eventId`.
- Projetar saldo diário por comerciante e data.
- Servir leitura otimizada do consolidado.
- Usar Redis como cache-aside.
- Invalidar cache ao processar evento da data afetada.

### Não responsabilidades

- Validar a regra de criação do lançamento.
- Consultar diretamente o banco do `lancamentos-service`.
- Corrigir ou reinterpretar o ledger.
- Calcular saldo acumulado.

### Modelo de dados principal

Database: `consolidado_db`.

Tabela `saldos_diarios`:

| Campo | Observação |
|---|---|
| `id` | UUID |
| `merchant_id` | UUID obrigatório |
| `data` | Data do consolidado |
| `total_creditos` | `numeric(18,2)` |
| `total_debitos` | `numeric(18,2)` |
| `saldo_dia` | `total_creditos - total_debitos` |
| `quantidade_lancamentos` | Contador de lançamentos processados |
| `atualizado_em` | Timestamp UTC |

Tabela `processed_events`:

| Campo | Observação |
|---|---|
| `event_id` | UUID único |
| `stream_message_id` | ID da mensagem no Redis Stream |
| `event_type` | Tipo do evento |
| `processed_at` | Timestamp UTC |

O serviço não mantém `saldoAcumulado`. Quando necessário, o acumulado deve ser calculado por quem consulta a série histórica, somando os dias retornados.

### Endpoints

Todos os endpoints de negócio exigem header `X-Merchant-Id` com UUID válido. Health checks não exigem esse header.

```text
GET    /api/v1/consolidados/{data}
GET    /api/v1/consolidados?de=&ate=
GET    /health/live
GET    /health/ready
```

Não haverá endpoint de reprocessamento no MVP. Esse recurso fica documentado como evolução administrativa.

## 7. Contrato de eventos

### Redis Stream

```text
stream: cashflow.lancamentos
consumer group: consolidado-service
```

### Envelope

Os eventos devem usar campos mínimos no Redis Stream e o corpo versionado em `payload`:

```text
eventId
eventType
eventVersion
occurredAt
payload
```

Exemplo:

```json
{
  "eventId": "7f72e78b-97f1-4a0e-89bc-f33a7b1440fd",
  "eventType": "LancamentoRegistrado",
  "eventVersion": 1,
  "occurredAt": "2026-07-02T10:30:00Z",
  "payload": {
    "merchantId": "00000000-0000-0000-0000-000000000001",
    "lancamentoId": "8a50af67-1cf7-4b6e-8024-7cf23f46d779",
    "tipo": "Credito",
    "valor": 150.75,
    "dataLancamento": "2026-07-02",
    "descricao": "Venda cartão",
    "categoria": "Vendas",
    "estornoDoLancamentoId": null
  }
}
```

Estorno não exige um evento especial para o consolidado. Ele deve ser publicado como outro `LancamentoRegistrado`, com tipo oposto, mesmo valor e `estornoDoLancamentoId` preenchido.

## 8. Processamento assíncrono

### Publicação pelo `lancamentos-service`

O worker interno do `lancamentos-service` deve:

1. Buscar eventos `Pending` em `outbox_events`.
2. Publicar cada evento no stream `cashflow.lancamentos`.
3. Marcar o evento como `Sent` após publicação bem-sucedida.
4. Incrementar tentativas e registrar erro em caso de falha.

Esse worker é uma simplificação adequada ao MVP. Em produção, a mesma responsabilidade poderia evoluir para um publisher dedicado ou CDC.

### Consumo pelo `consolidado-service`

O worker interno do `consolidado-service` deve:

1. Consumir via consumer group `consolidado-service`.
2. Verificar se `eventId` já existe em `processed_events`.
3. Se já existir, reconhecer a mensagem sem alterar saldo.
4. Se não existir, atualizar `saldos_diarios` e gravar `processed_events` na mesma transação.
5. Invalidar a chave de cache da data afetada.
6. Executar `XACK` somente depois do commit no PostgreSQL.

Como evolução, pode haver recuperação de mensagens pendentes com `XAUTOCLAIM` e uma estratégia formal para eventos que falham repetidamente.

## 9. Cache

O `consolidado-service` usa Redis como cache-aside para leitura:

```text
consolidado:{merchantId}:{yyyy-MM-dd}
```

Estratégia:

- `GET /api/v1/consolidados/{data}` tenta ler do cache.
- Em cache miss, busca no PostgreSQL e popula o cache.
- TTL curto sugerido: 30 a 60 segundos.
- Ao processar um evento, o consumidor remove a chave da data afetada.

Essa combinação reduz carga de leitura e evita que o cache esconda atualizações já processadas.

## 10. Persistência e execução local

O ambiente standalone usará um `docker-compose.yml` na raiz com:

- `frontend`
- `lancamentos-service`
- `consolidado-service`
- `postgres`
- `redis`

O Postgres local terá um único servidor com dois databases:

- `lancamentos_db`
- `consolidado_db`

Cada serviço acessa apenas o seu database. Isso mantém a separação de propriedade de dados sem aumentar a complexidade local.

As migrations do EF devem ser aplicadas automaticamente no startup dos serviços em ambiente local/MVP. Em produção, a recomendação é executar migrations em pipeline controlado antes do deploy da aplicação.

## 11. Autenticação no MVP

O MVP não implementará OIDC/JWT real. Para manter a separação por comerciante sem adicionar complexidade de identidade:

- endpoints de negócio exigem `X-Merchant-Id`;
- o header deve conter UUID válido;
- o frontend envia um UUID fixo via `NEXT_PUBLIC_MERCHANT_ID`;
- a documentação de arquitetura registra que, em produção, o `merchantId` viria das claims do token.

## 12. Testes

Escopo de testes do MVP: unitários no backend.

`lancamentos-service.Tests` deve cobrir:

- valor precisa ser positivo;
- data futura é rejeitada;
- data retroativa é aceita;
- estorno gera lançamento oposto;
- registro de lançamento cria evento de outbox.

`consolidado-service.Tests` deve cobrir:

- crédito incrementa `totalCreditos`;
- débito incrementa `totalDebitos`;
- `saldoDia` é calculado corretamente;
- evento duplicado não altera saldo;
- cache é invalidado após processamento, se essa regra estiver isolada em componente testável.

Testes de integração, carga e contrato ficam como evolução.

## 13. Fora do MVP

Itens intencionalmente fora do primeiro escopo executável:

- API Gateway/BFF executável.
- Autenticação OIDC/JWT real.
- Endpoint administrativo de reprocessamento.
- Testes com Testcontainers.
- Testes de carga com k6.
- Observabilidade completa com Prometheus/Grafana/Loki.
- Deploy Kubernetes/cloud.
- Migração para RabbitMQ, Azure Service Bus ou Kafka.

Esses itens seguem como decisões ou evoluções documentadas, mas o MVP deve priorizar o fluxo essencial: registrar lançamento, publicar evento de forma confiável, consumir de forma idempotente e consultar o saldo diário consolidado.
