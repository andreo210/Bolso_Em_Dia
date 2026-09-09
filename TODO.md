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

## Em andamento

_Nada em andamento no momento._

## A fazer

Falta a camada Application/Api dos demais casos de uso (Domain e Infra já prontos para todos):

### Transações
- [ ] UC03 — Registrar receita
- [ ] UC04 — Registrar despesa (inclui UC05; extend UC10)
- [ ] UC06 — Transferir entre contas (inclui UC05)

### Transações
- [ ] UC03 — Registrar receita
- [ ] UC04 — Registrar despesa (inclui UC05; extend UC10)
- [ ] UC06 — Transferir entre contas (inclui UC05)

### Categorias e orçamento
- [ ] UC07 — Cadastrar categoria
- [ ] UC08 — Definir orçamento mensal
- [ ] UC09 — Acompanhar progresso do orçamento
- [ ] UC10 — Alertar orçamento estourado

### Metas de economia
- [ ] UC11 — Criar meta de economia
- [ ] UC12 — Registrar aporte em meta

### Cartão de crédito
- [ ] UC13 — Cadastrar cartão de crédito
- [ ] UC14 — Registrar compra no cartão (inclui UC15, UC16)
- [ ] UC15 — Verificar limite disponível do cartão
- [ ] UC16 — Gerar parcelas da compra
- [ ] UC17 — Pagar fatura (inclui UC04)
- [ ] UC20 — Fechar fatura do ciclo (job agendado)

### Recorrências
- [ ] UC18 — Criar recorrência
- [ ] UC19 — Pausar / cancelar recorrência
- [ ] UC21 — Gerar ocorrência de recorrência (job agendado; inclui UC03, UC04 ou UC14)

### Infra / qualidade (fora dos casos de uso)
- [ ] Controller de autenticação (registro/login) — os controllers de negócio já são `[Authorize]`, mas não há endpoint para emitir o JWT ainda, então não dá para testar via Swagger sem token
- [ ] Projeto de testes automatizados (nenhum existe hoje)
- [ ] `.gitattributes` para normalizar line endings (evitar diff CRLF/LF em massa)
- [ ] Corrigir `README.md` (hoje é o placeholder padrão do GitHub, em UTF-16)
- [ ] Remover pastas vazias soltas (`Bolso_Em_Dia/` na raiz, `BolsoEmDia.Domain/NovaPasta/`)
