# Análise de requisitos

Este documento é o elo que faltava entre "o que o sistema faz" (`casos-de-uso.md`) e "como ele é construído" (`mer.md` em diante): a lista explícita de requisitos funcionais e não-funcionais, cada um rastreado até o caso de uso ou a decisão de arquitetura que o originou. Nenhum requisito aqui é inventado — cada RF referencia o UC correspondente em [`especificacao-casos-de-uso.md`](especificacao-casos-de-uso.md), e cada RNF referencia o trecho real de código/skill que o implementa.

## Requisitos funcionais

Um RF por caso de uso — a tabela é deliberadamente 1:1 com `casos-de-uso.md` para que nenhum UC fique sem requisito rastreável, e vice-versa.

| ID | Requisito | UC | Prioridade |
|---|---|---|---|
| RF01 | O sistema deve permitir que o usuário cadastre uma conta financeira (Corrente, Poupança, Carteira ou Investimento) com nome e saldo inicial. | UC01 | Essencial |
| RF02 | O sistema deve permitir editar o nome de uma conta e ativá-la/inativá-la, preservando as transações já lançadas. | UC02 | Essencial |
| RF03 | O sistema deve permitir registrar uma receita em uma conta, associada a uma categoria do tipo Receita. | UC03 | Essencial |
| RF04 | O sistema deve permitir registrar uma despesa em uma conta, associada a uma categoria do tipo Despesa, verificando o saldo disponível antes de concluir o lançamento. | UC04, UC05 | Essencial |
| RF05 | O sistema deve calcular e exibir o saldo atual (e o saldo previsto, considerando lançamentos futuros) de uma conta. | UC05 | Essencial |
| RF06 | O sistema deve permitir transferir valores entre duas contas do mesmo usuário de forma atômica (ambos os lados ocorrem ou nenhum ocorre). | UC06 | Essencial |
| RF07 | O sistema deve permitir cadastrar categorias de receita ou despesa, próprias do usuário. | UC07 | Essencial |
| RF08 | O sistema deve permitir definir um orçamento mensal por categoria de despesa. | UC08 | Importante |
| RF09 | O sistema deve permitir consultar o progresso do orçamento de uma categoria (quanto já foi gasto sobre o quanto foi planejado) no mês corrente. | UC09 | Importante |
| RF10 | O sistema deve alertar o usuário quando um lançamento de despesa fizer o total do mês ultrapassar o orçamento definido para a categoria, sem bloquear o lançamento. | UC04, UC10 | Importante |
| RF11 | O sistema deve permitir criar uma meta de economia com valor alvo. | UC11 | Desejável |
| RF12 | O sistema deve permitir registrar aportes em uma meta de economia, acumulando o progresso. | UC12 | Desejável |
| RF13 | O sistema deve permitir cadastrar um cartão de crédito com limite, dia de fechamento e dia de vencimento. | UC13 | Essencial |
| RF14 | O sistema deve permitir registrar uma compra no cartão de crédito, à vista ou parcelada, verificando o limite disponível antes de aprovar. | UC14, UC15 | Essencial |
| RF15 | O sistema deve calcular o limite disponível do cartão a partir do limite total menos faturas em aberto e parcelas futuras já comprometidas. | UC15 | Essencial |
| RF16 | O sistema deve gerar automaticamente as parcelas de uma compra parcelada, distribuídas nas faturas seguintes. | UC16 | Essencial |
| RF17 | O sistema deve fechar automaticamente a fatura do cartão na data de fechamento configurada, impedindo novos lançamentos na fatura fechada. | UC20 | Essencial |
| RF18 | O sistema deve permitir pagar a fatura fechada de um cartão, registrando o pagamento como despesa na conta de origem informada. | UC17, UC04 | Essencial |
| RF19 | O sistema deve permitir criar uma recorrência (receita, despesa ou compra de cartão) que se repete automaticamente em um intervalo configurado. | UC18 | Importante |
| RF20 | O sistema deve permitir pausar ou cancelar uma recorrência ativa, interrompendo a geração de novas ocorrências. | UC19 | Importante |
| RF21 | O sistema deve gerar automaticamente, na data configurada, a ocorrência de uma recorrência ativa como receita, despesa ou compra no cartão, conforme o tipo configurado. | UC21 | Importante |

**Sobre a coluna Prioridade:** "Essencial" é o que compõe o fluxo mínimo de uma ferramenta de controle financeiro (conta, lançamento, saldo, cartão, fatura); "Importante" agrega planejamento (orçamento, recorrência); "Desejável" é a meta de economia, que não bloqueia nem é pré-requisito de nenhum outro fluxo. Isso reflete a ordem de implementação observada no histórico do projeto (contas/transações → categorias/orçamento → cartão), não uma imposição deste documento.

## Requisitos não-funcionais

Cada RNF abaixo está ancorado em uma decisão de arquitetura real do projeto (código ou skill), não em boas práticas genéricas de mercado — onde a fonte é uma skill, ela é a referência para o comportamento completo.

| ID | Requisito | Categoria | Fonte |
|---|---|---|---|
| RNF01 | Toda rota da Api (exceto emissão/renovação de token e o endpoint público de chave JWKS) deve exigir autenticação via JWT Bearer assinado com RS256; a chave privada nunca sai do processo, só a pública é publicada em `GET /.well-known/jwks.json`. | Segurança | `BolsoEmDia.Api/Program.cs` (`AddAuthentication`/`AddJwtBearer`, `RsaKeyService`) |
| RNF02 | Todo dado financeiro (conta, transação, categoria, cartão, fatura, meta, recorrência) deve pertencer a exatamente um usuário (`IdUsuario`) e só pode ser lido/alterado por esse usuário — nenhuma operação cruza dados entre usuários. | Segurança / isolamento de dados | `ICurrentUser`/`CurrentUser` (`arquitetura-api`, `references/persistencia.md`); pré-condição implícita de todos os UCs (ver nota no topo de `especificacao-casos-de-uso.md`) |
| RNF03 | Senha de usuário deve ter no mínimo 6 caracteres; e-mail deve ser único no sistema. | Segurança | `BolsoEmDia.Api/Program.cs` (`AddIdentityCore` → `options.Password`, `options.User.RequireUniqueEmail`) |
| RNF04 | Falha de regra de negócio (saldo insuficiente, limite excedido, categoria do tipo errado etc.) deve ser sinalizada como notificação acumulada (`INotificadorService`) e devolvida como `400 ProblemDetails` (RFC 7807) com todos os erros da requisição de uma vez — nunca como exceção não tratada. | Confiabilidade / tratamento de erro | Skill `arquitetura-api`, regra 1 ("Falha de regra de negócio é notificação, não exceção") |
| RNF05 | Erro não previsto (bug, exceção de infraestrutura) deve ser capturado pelo `ExceptionMiddleware` e devolvido como `500 ProblemDetails`, sem vazar stack trace ao cliente. | Confiabilidade | `BolsoEmDia.Api` middleware (`ExceptionMiddleware`), skill `arquitetura-api` |
| RNF06 | Toda entidade deve proteger suas invariantes por construção (`private set`, construtor protegido, fábrica `static Criar(...)`) — não deve existir caminho de código que produza uma entidade em estado inválido (ex.: status fora do enum, valor negativo onde não é permitido). | Integridade de dados | Skill `arquitetura-api`, regra 2 ("A entidade protege as próprias invariantes") |
| RNF07 | Toda alteração de dado relevante deve manter auditoria (quem criou/alterou e quando), incluindo o caso de job agendado (`ICurrentUser.UserId ?? "SYSTEM"`). | Rastreabilidade | `arquitetura-api/references/persistencia.md` (`AplicarAuditoria`, `IAuditoria`, `TemporalEntity`) |
| RNF08 | Persistência deve ser feita em PostgreSQL via EF Core, através do repositório genérico (`RepositorioGlobal<T>`), com `AsNoTracking` por padrão em consultas de leitura. | Desempenho / arquitetura de dados | Skill `arquitetura-api`, regra 3 ("Persistência passa pelo `RepositorioGlobal`") |
| RNF09 | Operações que envolvem mais de uma escrita atômica (ex.: transferência entre contas, pagamento de fatura) devem ocorrer dentro de uma única transação de banco (`IUnitOfWork`) — ou todas as escritas persistem, ou nenhuma. | Consistência | `UnitOfWork`/`IUnitOfWork` (skill `arquitetura-api`); UC06, UC17 |
| RNF10 | A Api deve expor documentação OpenAPI/Swagger navegável, incluindo o esquema de autenticação Bearer e os comentários XML dos endpoints. | Manutenibilidade / documentação | `BolsoEmDia.Api/Program.cs` (`AddSwaggerGen`, `IncludeXmlComments`) |
| RNF11 | O front-end deve consumir a Api exclusivamente via HTTP (`ApiHttpService`), sem acesso direto ao banco de dados a partir do Blazor Server. | Arquitetura / separação de camadas | Skill `arquitetura-front`; projeto `BolsoEmDia.Front.Services` |
| RNF12 | Entidades de domínio, propriedades e mensagens de validação devem ser nomeadas em português, consistente com o restante do código (`IdUsuario`, `ValorDiaria`, etc.). | Manutenibilidade / consistência | `CLAUDE.md` — "Code style" |
| RNF13 | O job agendado de fechamento de fatura (UC20) e de geração de ocorrência de recorrência (UC21) deve rodar sem intervenção do usuário, na data configurada, e reutilizar os mesmos métodos de domínio/serviço usados pelo fluxo manual equivalente (não duplicar a regra de negócio). | Automação / não-duplicação de regra | `casos-de-uso.md` (ator `SistemaJob`); notas de UC17/UC21 em `especificacao-casos-de-uso.md` |

> Zona cinzenta — não confirmada no código: não há, até este documento, requisito de desempenho quantitativo (tempo de resposta, volume esperado de transações por usuário) nem de disponibilidade (SLA, backup) formalizado em código ou skill. Se o produto tiver uma meta concreta (ex.: "listagem de transações em até 500ms com 10k lançamentos"), confirme com quem define o requisito antes de tratá-lo como compromisso — hoje só a paginação do `RepositorioGlobal` (skill `arquitetura-api`) existe como mitigação de volume.

## Rastreabilidade

- Todo RF aponta para um UC em [`casos-de-uso.md`](casos-de-uso.md) / [`especificacao-casos-de-uso.md`](especificacao-casos-de-uso.md) — não há requisito funcional sem caso de uso correspondente, nem UC sem RF.
- Todo RNF aponta para código real ou para uma skill deste repositório — não há requisito não-funcional baseado em prática de mercado genérica não confirmada no projeto.
- Se um RF ou RNF mudar, atualize também o UC/skill de origem (ou vice-versa) para as duas fontes não divergirem.
