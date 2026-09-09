---
name: testes-automatizados
description: Cria e organiza testes automatizados (unitários, de serviço/aplicação, de integração) para qualquer linguagem ou framework. Funciona explorando o projeto-alvo — framework de teste já em uso, convenção de nome, fakes vs. mock, como o código sinaliza erro de regra de negócio — em vez de aplicar um template fixo. Use sempre que o usuário pedir para escrever/criar testes, adicionar cobertura, testar uma regra de negócio, montar um projeto de testes do zero, ou perguntar "como eu testo isso" — mesmo sem usar a palavra "teste" explicitamente, por exemplo "garante que essa validação não quebra depois" ou "quero ter mais confiança pra mexer nesse código".
---

# Testes automatizados

O valor desta skill não é "sei a sintaxe do xUnit/pytest/Jest" — é o processo antes de escrever: **descobrir como o projeto-alvo já testa (ou decidir isso de forma sensata quando ainda não testa nada) antes de inventar um padrão novo.** Um teste que usa Moq num projeto que só usa fakes em memória, ou que testa `GetSaldo()` quando o método real é `ObterSaldo()`, é atrito — ensina o próximo dev a copiar o padrão errado.

## Passo 1 — Descubra o terreno antes de escrever

**Se já existe projeto/pasta de teste:** leia testes reais existentes antes de escrever o primeiro novo. Procure especificamente por:

- Framework e runner (xUnit/NUnit/MSTest, pytest/unittest, Jest/Vitest, JUnit, RSpec, `go test`, etc.) e como ele é invocado (`dotnet test`, `pytest`, `npm test`...).
- Convenção de nome de arquivo, classe e método de teste — e se o nome descreve comportamento (`Nao_pode_locar_bloqueado`) ou segue outro estilo (`should_...`, `test_...`, `it("...")`). Siga o que já existe, não o que você acharia mais elegante.
- **Fake escrito à mão vs. biblioteca de mock.** Muitos projetos .NET/Java maduros preferem uma implementação em memória da interface (ex.: `RepositorioFake<T>`) a Moq/NSubstitute, porque testa o contrato real em vez de "gravar" chamadas esperadas. Outros usam mock/spy de propósito (`jest.mock`, `unittest.mock`, `Moq`). Não misture as duas abordagens no mesmo projeto sem necessidade — replique a que já está lá.
- **Como o código sinaliza falha de regra de negócio**: exceção customizada, objeto de notificação acumulado (`notificador.TemNotificacao()`), `Result<T>`/`Either`, ou retorno nulo? O teste tem que assertar contra o mecanismo real do projeto — não assumir `try/catch` genérico se o projeto usa notificador, por exemplo.
- Se há um lugar central de dados de teste válidos (fábrica/builder, ex. `Fabrica.Cliente()`, `factory_boy`, `test/fixtures`) — se sim, use-o e estenda-o em vez de montar a entidade na mão dentro de cada teste.
- Se há uma skill ou documento de arquitetura/regras de negócio no repositório (ex. `regras-negocio-financas`, `arquitetura-api`) — trate como fonte da verdade sobre o que é uma regra crítica que merece teste prioritário; não redescubra a regra de negócio só lendo o código quando ela já está documentada.

**Se não existe nenhuma infraestrutura de teste ainda:** não tem o que explorar no próprio projeto, então:
1. Identifique a linguagem/ecossistema e use o framework padrão de mercado dele (tabela no Passo 5) — não introduza uma dependência exótica sem motivo.
2. Se houver ambiguidade real de escolha (ex.: Jest vs. Vitest num projeto Node novo, pytest puro vs. com plugins), pergunte ao usuário em vez de decidir sozinho — é uma decisão que vai acompanhar o projeto.

## Passo 2 — Decida o que vale a pena testar

Cobertura alta não é o objetivo; confiança em código que quebra silenciosamente é. Priorize:

- Regras de negócio e invariantes de domínio (o que a entidade proíbe, não só o que ela permite).
- Transições de estado (o que pode e o que não pode sair de cada estado).
- Casos de borda das regras acima (limite exato, valor zero/negativo, data no limite, coleção vazia).
- Caminhos de erro — o que acontece quando a regra é violada, não só o caminho feliz.
- Código que já quebrou antes ou que tem histórico de bug — vale um teste de regressão específico.

Pule ou dê baixa prioridade a:
- Getters/setters triviais e DTOs anêmicos sem comportamento.
- Código gerado ou wrappers finos de framework (ex.: um controller que só repassa pra um service, sem lógica própria).
- Testar a mesma regra em três camadas diferentes quando testar na camada onde ela realmente vive já garante a proteção.

Se o usuário pediu "cobertura" de algo específico, confirme com ele o recorte antes de sair testando o projeto inteiro — é barato alinhar escopo antes, caro escrever 40 testes fora do que interessava.

## Passo 3 — Estrutura de um teste bom

- **Nome descreve comportamento esperado**, não implementação — na convenção que o projeto já usa (Passo 1). Alguém lendo só a lista de nomes dos testes de uma classe deveria entender as regras dela sem abrir o código.
- **Arrange / Act / Assert** com separação visual clara (linha em branco entre as partes), mesmo quando a linguagem não tem esses comentários explícitos.
- **Dados de teste passam só o que importa para aquele caso.** Um builder/fábrica central com valores padrão válidos evita que toda entidade seja reconstruída à mão em cada teste — e quando uma validação nova entra na entidade, corrige-se um lugar só em vez de N testes quebrados.
- **Isolamento de dependência externa** (banco, fila, API, relógio) segue o padrão já escolhido no Passo 1. Se for escrever um fake novo porque nenhum existe ainda: implemente só o contrato da interface, documente no próprio fake o que ele conscientemente não reproduz (ex.: "não faz tracking de mudança, verifique a chamada de salvar, não o conteúdo"), para que quem usar depois não confie nele além do que ele cobre.
- **Comentário só quando explica um "porquê" não óbvio** — uma data fixada porque a entidade valida contra `UtcNow`, um id escrito via reflection porque o setter é privado, um motivo pra não usar `DateTime.Now`. Não comente o que o nome do teste já diz.
- Prefira `[Theory]`/`@pytest.mark.parametrize`/`it.each` (o equivalente do framework) quando o mesmo comportamento se repete com várias entradas — evita cópia de teste quase idêntico.

## Passo 4 — Escreva, rode de verdade, itere

Escrever o teste não é o fim — **rodar o comando de teste do projeto e ver passar é parte obrigatória da tarefa**, não um "depois eu confiro". Se o comando de teste do projeto não é óbvio, é a primeira coisa a checar no Passo 1 (arquivo de skill/README do projeto costuma ter isso documentado).

Se um teste falha por um motivo diferente do que ele deveria verificar (setup incompleto, dependência faltando), conserte o setup — não enfraqueça a asserção pra fazer passar.

Para testes de regra crítica, vale a pena confirmar que o teste de fato pega o erro: comente/quebre temporariamente a regra na implementação, rode o teste e confirme que ele falha, desfaça a quebra. Um teste que passa incondicionalmente (verde mesmo com a regra quebrada) é pior do que nenhum teste — ele passa segurança falsa.

## Passo 5 — Criando infraestrutura de teste do zero

Quando o projeto ainda não tem nada, use o padrão do ecossistema — não é o momento de escolher algo exótico:

| Ecossistema | Framework padrão | Comando típico |
|---|---|---|
| .NET (C#) | xUnit (ou NUnit/MSTest se já for convenção da empresa) | `dotnet new xunit -o Projeto.Tests` + `dotnet test` |
| Node/TypeScript | Vitest (projetos novos/Vite) ou Jest (mais estabelecido) | `npm test` |
| Python | pytest | `pytest` |
| Go | pacote `testing` nativo (+ `testify` para asserções, se o projeto já usa) | `go test ./...` |
| Java/Kotlin | JUnit 5 | `mvn test` / `gradle test` |
| Ruby | RSpec | `bundle exec rspec` |

Depois de criar a infraestrutura, espelhe as camadas do projeto em vez de jogar tudo numa pasta `tests/` plana: teste de domínio/unidade separado de teste de serviço/aplicação (que usa fake/mock das dependências) separado de teste de integração (que toca banco/API de verdade e por isso é mais caro e mais raro). Isso deixa claro, só pelo caminho do arquivo, o que cada teste está de fato verificando.
