# TODO — BolsoEmDia

Status das tarefas do projeto. Casos de uso (UC) referenciam `docs/modelagem/casos-de-uso.md`.

## Concluído

- [x] Arquitetura base da solução (Domain → Infra → Application → Api, + Front/Front.Services/Front.Models)
- [x] Padrão de repositório genérico (`IRepositorioGlobal`, `RepositorioGlobal`, `UnitOfWork`)
- [x] Padrão de notificação de erros de negócio (`NotificadorService`, sem exceptions para regra de negócio)
- [x] `MainController` base (traduz notificação em `ProblemDetails`) + `ExceptionMiddleware`
- [x] Autenticação: `ApplicationUser` + ASP.NET Identity (tabelas `AspNetUsers`, `AspNetRoles` etc.) + configuração JWT
- [x] `AppDbContext` + `AppDbContextFactory` + migration inicial (`Inicial`)
- [x] Documentação de modelagem completa: casos de uso, MER, DER, UML de classes, diagrama de estados, diagramas de sequência, dicionário de dados (`docs/modelagem/`)
- [x] `CLAUDE.md` + skills de arquitetura (`arquitetura-api`, `arquitetura-front`, `regras-negocio-financas`, `commit-seguro`, `documentacao-modelagem`)
- [x] Lint: `.editorconfig` + `Directory.Build.props` (analisadores .NET habilitados)
- [x] Entidades de domínio dos 21 casos de uso, interfaces de repositório, configs EF, repositórios concretos, `DbSet`s no `AppDbContext`
- [x] Migration `EntidadesFinanceiras` (tabelas das 12 entidades novas)
- [x] UC01 — Cadastrar conta
- [x] UC02 — Editar / ativar / inativar conta
- [x] UC05 — Verificar saldo da conta
- [x] Controller de autenticação (registro/login) — `AutenticacaoController` (`POST /api/v1/auth/registrar`, `POST /api/v1/auth/login`)
- [x] UC03 — Registrar receita — `TransacaoService`/`TransacaoController` (`POST /api/v1/transacoes/receitas`)
- [x] Projeto de testes automatizados (`BolsoEmDia.Tests`, xUnit — cobre autenticação, UC01, UC02, UC05, UC03, UC04, UC06)
- [x] UC07 — Cadastrar categoria — `CategoriaService`/`CategoriaController` (`POST /api/v1/categorias`)
- [x] UC08 — Definir orçamento mensal — `OrcamentoService`/`OrcamentoController` (`POST /api/v1/orcamentos`)
- [x] UC09 — Acompanhar progresso do orçamento — `OrcamentoService`/`OrcamentoController` (`GET /api/v1/orcamentos/progresso`)
- [x] UC04 — Registrar despesa — `TransacaoService`/`TransacaoController` (`POST /api/v1/transacoes/despesas`, inclui UC05, estende UC10 via `OrcamentoService.VerificarEstouroAsync`)
- [x] UC10 — Alertar orçamento estourado — implementado junto de UC04 (`OrcamentoService.VerificarEstouroAsync`)
- [x] UC06 — Transferir entre contas — `TransferenciaService`/`TransferenciaController` (`POST /api/v1/transferencias`, inclui UC05, grava as duas pernas via `IUnitOfWork.ExecuteTransactionAsync`)
- [x] UC11 — Criar meta de economia — `MetaEconomiaService`/`MetaEconomiaController` (`POST /api/v1/metas`)
- [x] UC12 — Registrar aporte em meta — `MetaEconomiaService`/`MetaEconomiaController` (`POST /api/v1/metas/{id}/aportes`)
- [x] UC13 — Cadastrar cartão de crédito — `CartaoService`/`CartaoController` (`POST /api/v1/cartoes`)
- [x] UC14 — Registrar compra no cartão — `CompraService`/`CompraController` (`POST /api/v1/compras`, inclui UC15 e UC16)
- [x] UC15 — Verificar limite disponível do cartão — implementado junto de UC14 (`IParcelaRepository.ObterTotalParcelasNaoPagasAsync` + `Cartao.LimiteDisponivel`)
- [x] UC16 — Gerar parcelas da compra — implementado junto de UC14 (`Compra.Registrar` + `Cartao.ObterOuAbrirFaturaParaLancamento`)
- [x] UC17 — Pagar fatura — `FaturaService`/`FaturaController` (`POST /api/v1/faturas/{id}/pagamento`, inclui UC04; `Fatura.RegistrarPagamento` passou a aceitar Aberta→Paga para pagamento antecipado, ver diagrama-estados.md)

## Em andamento

_Nada em andamento no momento._

## A fazer

Falta a camada Application/Api dos demais casos de uso (Domain e Infra já prontos para todos):

### Cartão de crédito
- [ ] UC20 — Fechar fatura do ciclo (job agendado)

### Recorrências
- [ ] UC18 — Criar recorrência
- [ ] UC19 — Pausar / cancelar recorrência
- [ ] UC21 — Gerar ocorrência de recorrência (job agendado; inclui UC03, UC04 ou UC14)

### Infra / qualidade (fora dos casos de uso)
- [ ] `.gitattributes` para normalizar line endings (evitar diff CRLF/LF em massa)
- [ ] Corrigir `README.md` (hoje é o placeholder padrão do GitHub, em UTF-16)
- [ ] Remover pastas vazias soltas (`Bolso_Em_Dia/` na raiz, `BolsoEmDia.Domain/NovaPasta/`)
