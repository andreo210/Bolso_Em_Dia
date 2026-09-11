# BolsoEmDia

Aplicativo de organização financeira pessoal — contas, transações, categorias e orçamento mensal, metas de economia, cartão de crédito (compra, fatura, fechamento, limite, parcelamento) e transações recorrentes/assinaturas.

Solução .NET 10, arquitetura em camadas (DDD), com front end em Blazor Server.

## Projetos

| Projeto | Responsabilidade |
|---|---|
| `BolsoEmDia.Domain` | Entidades, invariantes de negócio, contratos de repositório. Sem dependência de EF/ASP.NET. |
| `BolsoEmDia.Infra` | Persistência com EF Core, repositório genérico, `AppDbContext`, migrations. |
| `BolsoEmDia.Application` | Serviços de aplicação, DTOs, mappers, notificador de erros de negócio. |
| `BolsoEmDia.Api` | Web API (ASP.NET), controllers, autenticação JWT, Swagger. |
| `BolsoEmDia.Front` | Front end Blazor Server. |
| `BolsoEmDia.Front.Services` | Consumo HTTP da Api a partir do front (`ApiHttpService`), tratamento de token. |
| `BolsoEmDia.Front.Models` | Contratos de request/response + validadores FluentValidation compartilhados com o front. |
| `BolsoEmDia.Tests` | Testes automatizados (xUnit). |

## Como rodar

Pré-requisitos: .NET 10 SDK e um Postgres acessível (local via Docker, por exemplo).

```bash
dotnet build
dotnet run --project BolsoEmDia.Api
dotnet run --project BolsoEmDia.Front
```

A connection string padrão (`Host=localhost;Port=5432;Database=bolso_em_dia`) está em `BolsoEmDia.Api/appsettings.json`. Para outro ambiente, defina a variável `ConnectionStrings__BolsoEmDia`.

## Documentação

- [`CLAUDE.md`](CLAUDE.md) — visão geral do projeto, convenções de código e fluxo de trabalho.
- [`TODO.md`](TODO.md) — status dos casos de uso e pendências.
- [`docs/modelagem/`](docs/modelagem/) — modelagem completa do domínio: requisitos, casos de uso, MER/DER, diagrama de classes UML, diagrama de estados, diagramas de sequência e dicionário de dados.
- `.claude/skills/` — skills com as regras de arquitetura (`arquitetura-api`, `arquitetura-front`) e de negócio (`regras-negocio-financas`) usadas como fonte de verdade ao implementar novas funcionalidades.
