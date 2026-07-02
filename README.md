# Controle de Fluxo de Caixa

MVP para controle de lançamentos financeiros e saldo diário consolidado, usando dois serviços backend desacoplados por Redis Streams e uma interface web.

## Projetos

- `src/frontend`: Next.js + TypeScript para cadastrar, listar, estornar lançamentos e consultar consolidado.
- `src/lancamentos-service`: .NET 10 Minimal API, EF Core, ledger append-only e transactional outbox.
- `src/consolidado-service`: .NET 10 Minimal API, consumidor Redis Streams, projeção diária idempotente e cache Redis.

## Rodar standalone com Docker

Pré-requisito: Docker com Docker Compose.

```bash
./scripts/start.sh
```

Serviços expostos:

- Frontend: http://localhost:3000
- Lançamentos API: http://localhost:5001
- Consolidado API: http://localhost:5002
- Postgres: localhost:5432
- Redis: localhost:6379

Para parar:

```bash
./scripts/stop.sh
```

Para remover volumes e reiniciar bancos/cache do zero:

```bash
./scripts/reset.sh
```

## Rodar em desenvolvimento

Suba Postgres e Redis pelo devcontainer ou por Docker Compose e rode os apps manualmente. Fora de containers, os defaults usam `localhost`. Dentro do devcontainer, use os hostnames `db` e `redis`:

```bash
ConnectionStrings__LancamentosDb="Host=db;Port=5432;Database=lancamentos_db;Username=postgres;Password=postgres" \
Redis__ConnectionString=redis:6379 \
dotnet run --project src/lancamentos-service/LancamentosService.csproj

ConnectionStrings__ConsolidadoDb="Host=db;Port=5432;Database=consolidado_db;Username=postgres;Password=postgres" \
Redis__ConnectionString=redis:6379 \
dotnet run --project src/consolidado-service/ConsolidadoService.csproj

cd src/frontend && npm run dev
```

As migrations do EF são aplicadas automaticamente no startup dos backends em ambiente local/MVP. Os databases `lancamentos_db` e `consolidado_db` são criados se ainda não existirem.

## Testes

```bash
dotnet test Cashflow.slnx
cd src/frontend && npm run typecheck
cd src/frontend && npm run build
```

## Merchant ID

O MVP usa o header `X-Merchant-Id` para identificar o comerciante. O frontend envia por padrão:

```text
00000000-0000-0000-0000-000000000001
```

Em produção, esse valor deve vir das claims de um token OIDC/JWT.

## Documentação

- [Solução arquitetural](docs/02-solucao-arquitetural.md)
- [Plano de implementação](docs/03-plano-de-implementacao.md)
