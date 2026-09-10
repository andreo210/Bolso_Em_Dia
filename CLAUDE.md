# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

BolsoEmDia — personal finance management app (Portuguese-language codebase). Solution with 7 .NET 10 projects, layered DDD-style, plus a Blazor Server front end:

- `BolsoEmDia.Domain` — entities, invariants, repository contracts. No EF/ASP.NET dependencies.
- `BolsoEmDia.Infra` — EF Core persistence, generic repository, `AppDbContext`, migrations.
- `BolsoEmDia.Application` — application services, DTOs, mappers, notifier (accumulates rule failures instead of throwing).
- `BolsoEmDia.Api` — ASP.NET Web API, controllers inherit `MainController`, JWT auth, Swagger.
- `BolsoEmDia.Front` — Blazor Server UI.
- `BolsoEmDia.Front.Services` — HTTP consumption of the API from the front (`ApiHttpService`), token handling.
- `BolsoEmDia.Front.Models` — request/response contracts + FluentValidation validators shared with the front.

## Architecture & domain rules — read the skills, not just the code

This repo has established skills that are the source of truth for how to build things here; they load automatically when relevant:

- **arquitetura-api** — layered API architecture (Domain→Infra→Application→Api), generic repository, notifier pattern instead of exceptions, ProblemDetails responses.
- **arquitetura-front** — Blazor Server conventions: `TabelaGenerica`, `ApiHttpService`, `EditForm` + FluentValidation.
- **regras-negocio-financas** — business rules for accounts, transactions, categories/budgets, credit card invoices, recurring transactions. Consult before touching any of that domain logic.
- **documentacao-modelagem** — generates the Mermaid docs under `docs/modelagem/`.
- **commit-seguro** — safe commit workflow for this repo (see Line endings below).

`docs/modelagem/` has the current data/domain model (use cases, MER, DER, UML classes, state diagram, sequence diagrams) — check it before creating or changing an entity, migration, or `AppDbContext` config. The `regras-negocio-financas` skill is the source of truth if a diagram and the skill ever disagree.

## Code style (differs from plain C# defaults)

- Block namespaces (`namespace X { ... }`), not file-scoped, across all projects.
- Nullable reference types enabled project-wide.
- Entities: private setters, protected/private parameterless constructor (EF requirement), static `Criar(...)` factory method for construction — no public setters on entities.
- Domain property/entity names are in Portuguese (`IdUsuario`, `ValorDiaria`, `Mensagem`, etc.) — keep new code consistent with this, don't switch to English.

## Build/run

No test project exists yet in the solution. Standard commands apply:
- `dotnet build`
- `dotnet run --project BolsoEmDia.Api`
- `dotnet run --project BolsoEmDia.Front`

## Local setup

Postgres runs in a manually-started Docker container; connection string and credentials are in `BolsoEmDia.Api/appsettings.json` (`Host=localhost;Port=5432;Database=bolso_em_dia`). The EF design-time factory (`AppDbContextFactory`) reads `ConnectionStrings__BolsoEmDia` from the environment if set, otherwise falls back to that same local default.

## Line endings

Committed files use CRLF, but there's no `.gitattributes` enforcing it, so editing from WSL can flip a file to LF and produce a repo-wide whitespace-only diff. Use the `commit-seguro` skill when committing to avoid this.

## Git workflow

Work happens on `andre-dev`; open PRs from `andre-dev` into `main`.
