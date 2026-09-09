# Especificação de casos de uso

O diagrama em [`casos-de-uso.md`](casos-de-uso.md) mostra *quem* aciona *o quê* e como os 21 casos de uso se relacionam (`«include»`/`«extend»`). Este documento é o detalhamento textual de cada um: ator, pré-condições, fluxo principal passo a passo, fluxos alternativos/exceção e pós-condições — o nível de detalhe que dá para conferir contra uma implementação.

Os nomes de método (`Conta.PermiteSaldoNegativo()`, `Fatura.AceitaNovoLancamento()` etc.) são os reais do diagrama de classes ([`uml-classes.md`](uml-classes.md)); a ordem exata de chamada entre Controller → Service → entidade → repositório dos seis fluxos mais sensíveis já está desenhada em [`diagramas-sequencia.md`](diagramas-sequencia.md) — aqui não repetimos aquele diagrama, apontamos para ele. A razão de cada restrição (bloqueia vs. avisa, por que um campo é obrigatório) é a skill `regras-negocio-financas`; se este documento e a skill um dia divergirem, a skill é a fonte da verdade.

**Convenção usada nas pré-condições/pós-condições:** "usuário autenticado" e "o recurso pertence ao `IdUsuario` autenticado" são pré-condições implícitas de todo caso de uso acionado pelo ator Usuário — omitidas caso a caso abaixo para não repetir 21 vezes; só está escrito quando há algo além disso.

---

## UC01 — Cadastrar conta

**Ator:** Usuário

**Pré-condições:** nenhuma além da autenticação — é o primeiro cadastro possível no domínio.

**Fluxo principal:**
1. Usuário informa nome, tipo (`Corrente`, `Poupança`, `Carteira` ou `Investimento`) e saldo inicial.
2. Sistema chama `Conta.Criar(idUsuario, nome, tipo, saldoInicial)`.
3. Sistema persiste a conta como `Ativa`.

**Fluxos alternativos / exceção:**
- **E1 — dados inválidos** (nome vazio, tipo fora do enum): o notificador acumula os erros de validação e a operação não é persistida; resposta `400 ProblemDetails` (padrão descrito em `arquitetura-api`).

**Pós-condições:** nova `Conta` ativa existe, com saldo inicial igual a `saldoInicial` e nenhuma transação lançada; a conta já pode ser usada como origem/destino em UC03–UC06.

---

## UC02 — Editar / inativar conta

**Ator:** Usuário

**Pré-condições:** conta existente e pertencente ao usuário.

**Fluxo principal:**
1. Usuário altera o nome e/ou o status da conta.
2. Sistema chama `Conta.Renomear(nome)` e/ou `Conta.Ativar()`/`Conta.Desativar()`.
3. Sistema persiste a alteração.

**Fluxos alternativos / exceção:**
- **E1 — nome inválido**: mesmo tratamento de erro do UC01 (notificador → `400`).

**Pós-condições:** conta atualizada; se inativada, deixa de aparecer como opção de origem/destino para novos lançamentos (UC03/UC04/UC06), mas as transações já existentes **não são apagadas nem alteradas**.

> Zona cinzenta — não confirmada no código: não há regra explícita bloqueando `Desativar()` numa conta com saldo diferente de zero (positivo ou, numa `Corrente`, negativo). Se o produto exigir "zerar antes de inativar", confirme antes de implementar — hoje o método aceita o estado como está.

---

## UC03 — Registrar receita

**Ator:** Usuário (diretamente) ou Sistema (via UC21 — geração de recorrência com `IdConta` preenchido)

**Pré-condições:** conta ativa; categoria do tipo `Receita` ativa e pertencente ao usuário.

**Fluxo principal:**
1. Usuário informa conta, categoria, data, valor (sempre positivo) e, opcionalmente, descrição.
2. Sistema valida que a categoria é do tipo `Receita` — categoria de despesa é rejeitada aqui.
3. Sistema chama `Transacao.RegistrarReceita(idConta, idCategoria, data, valor, descricao)`.
4. Sistema persiste a transação.

Diferente de UC04, receita **nunca** passa por verificação de saldo — só soma.

**Fluxos alternativos / exceção:**
- **E1 — categoria do tipo errado, valor ≤ 0, conta inativa**: notificador acumula erro, `400 ProblemDetails`.

**Pós-condições:** nova `Transacao` do tipo `Receita` persistida. Se `Data <= hoje`, o saldo atual da conta aumenta em `Valor`; se `Data` é futura, a transação fica agendada e só entra no saldo previsto até a data chegar (ver `contas-e-transacoes.md` da skill de regras).

---

## UC04 — Registrar despesa

**Ator:** Usuário (diretamente), Sistema (via UC17 — pagar fatura, e via UC21 — geração de recorrência com `IdConta` preenchido)

**Inclui:** UC05 — Verificar saldo da conta (sempre acontece, não é opcional)
**Estende:** UC10 — Alertar orçamento estourado (só dispara se a categoria tiver orçamento definido no mês e o total já tiver passado da meta)

**Pré-condições:** conta ativa; categoria do tipo `Despesa` ativa e pertencente ao usuário.

**Fluxo principal** (passo a passo completo, com nomes de método reais, no diagrama de sequência 1 de [`diagramas-sequencia.md`](diagramas-sequencia.md)):
1. Usuário informa conta, categoria, data, valor, descrição.
2. Sistema executa UC05 (`Conta.PermiteSaldoNegativo()` + cálculo do saldo resultante).
3. Se o saldo permite, `Transacao.RegistrarDespesa(...)` é chamado e persistido.
4. Sistema executa a checagem de orçamento (UC10) **depois** de persistir a despesa — o alerta nunca bloqueia o passo 3.
5. Resposta `201 Created`, com `AlertaOrcamento` preenchido no DTO se UC10 disparou.

**Fluxos alternativos / exceção:**
- **E1 — saldo insuficiente** (conta `Poupança`/`Carteira`/`Investimento` cujo saldo resultante seria `< 0`): notificador acumula "Saldo insuficiente", nada é persistido, `400 ProblemDetails`. Numa conta `Corrente` este fluxo nunca dispara — ela sempre permite negativar.
- **A1 — orçamento estourado (UC10)**: não é um erro; a despesa é aceita normalmente e a resposta `201` carrega um aviso informativo. Ver a nota de arquitetura no diagrama de sequência 1 sobre por que isso **não** passa pelo `INotificadorService`.

**Pós-condições:** nova `Transacao` do tipo `Despesa` persistida; saldo da conta reduzido em `Valor` (se `Data <= hoje`); progresso do orçamento da categoria no mês (UC09) recalculado na próxima consulta.

---

## UC05 — Verificar saldo da conta

**Ator:** nenhum ator direto — caso de uso interno, sempre acionado via `«include»` por UC04 (despesa) e UC06 (transferência), nunca pelo usuário isoladamente.

**Pré-condições:** conta identificada e o valor da operação que a está chamando.

**Fluxo principal:**
1. Sistema consulta `Conta.PermiteSaldoNegativo()` — verdadeiro só para `Corrente`.
2. Se `false` (Poupança/Carteira/Investimento), sistema calcula `saldoResultante = SaldoAtual(conta) - valor` — comparação exata, sem tolerância de centavos.
3. Devolve ao caso de uso chamador se a operação pode prosseguir.

**Fluxos alternativos / exceção:** nenhum próprio — quem decide o que fazer com o resultado (bloquear e notificar) é o caso de uso que incluiu este (UC04, UC06).

**Pós-condições:** nenhuma alteração de estado — é uma consulta pura, sem efeito colateral.

---

## UC06 — Transferir entre contas

**Ator:** Usuário

**Inclui:** UC05 — Verificar saldo da conta (aplicado à conta de origem, como se fosse uma despesa)

**Pré-condições:** conta de origem e conta de destino existentes, ativas e distintas.

**Fluxo principal** (diagrama de sequência 2 de `diagramas-sequencia.md`):
1. Usuário informa conta de origem, conta de destino, data, valor e descrição.
2. Sistema executa UC05 sobre a conta de origem.
3. Se o saldo permite, `IUnitOfWork.ExecuteTransactionAsync` abre uma transação de banco e chama `Transferencia.Registrar(idContaOrigem, idContaDestino, data, valor, descricao)`, que internamente cria as duas pernas via `Transacao.CriarPernaTransferencia` (`internal` — ninguém cria essa perna por fora).
4. As duas pernas e a `Transferencia` são persistidas **juntas**, no mesmo commit.

**Fluxos alternativos / exceção:**
- **E1 — saldo insuficiente na origem**: mesma regra de UC05/UC04; notificador, `400 ProblemDetails`, nenhuma perna é criada.
- **E2 — falha ao persistir qualquer uma das duas pernas**: `ROLLBACK` da transação de banco inteira — nunca fica só uma perna gravada; erro propaga como `500`/`ProblemDetails` via middleware.

**Pós-condições:** uma `Transferencia` e exatamente duas `Transacao` (`TransferenciaSaida` e `TransferenciaEntrada`) persistidas atomicamente; saldo da origem reduzido, saldo do destino aumentado, ambos em `Valor`. A transferência **não** tem categoria e não entra em relatórios de "despesas/receitas por categoria" (ver skill de regras).

---

## UC07 — Cadastrar categoria

**Ator:** Usuário

**Pré-condições:** se informada `IdCategoriaPai`, a categoria-pai deve existir, pertencer ao usuário e ser do mesmo `Tipo` (a hierarquia tem só um nível — subcategoria não pode ter filha própria).

**Fluxo principal:**
1. Usuário informa nome, tipo (`Receita` ou `Despesa`) e, opcionalmente, categoria-pai.
2. Sistema chama `Categoria.Criar(idUsuario, nome, tipo, idCategoriaPai)`.
3. Sistema persiste a categoria como `Ativa`.

**Fluxos alternativos / exceção:**
- **E1 — categoria-pai de tipo diferente ou já é ela própria uma subcategoria**: notificador, `400 ProblemDetails`.

**Pós-condições:** nova `Categoria` ativa, disponível para uso em UC03/UC04 (transações do mesmo tipo) e UC08 (orçamento, se `Despesa`).

---

## UC08 — Definir orçamento mensal

**Ator:** Usuário

**Pré-condições:** categoria existente, pertencente ao usuário e do tipo `Despesa` — categoria de receita não tem orçamento.

**Fluxo principal:**
1. Usuário informa categoria, mês de referência e valor-meta.
2. Sistema chama `Orcamento.Definir(idUsuario, idCategoria, mesReferencia, valorMeta)` (ou `AlterarMeta(valorMeta)` se já existir orçamento daquela categoria naquele mês).
3. Sistema persiste o orçamento.

**Fluxos alternativos / exceção:**
- **E1 — categoria do tipo `Receita`**: notificador, `400 ProblemDetails`.

**Pós-condições:** `Orcamento` definido para aquela categoria/mês; passa a ser consultado por UC09 e UC10. Não há rollover — o teto vale só para aquele mês, sobra ou estouro não afeta o mês seguinte.

---

## UC09 — Acompanhar progresso do orçamento

**Ator:** Usuário

**Pré-condições:** existe `Orcamento` definido para a categoria e mês consultados — se não existir, não há "progresso" a mostrar (trate como ausência, nunca como 0% ou 100%+).

**Fluxo principal:**
1. Usuário abre a tela/consulta de orçamento de uma categoria num mês.
2. Sistema soma as despesas daquela categoria (e subcategorias, se houver) com `Data <= hoje` dentro do mês de referência.
3. Sistema chama `Orcamento.PercentualConsumido(totalGastoNoMes)`.
4. Sistema devolve o percentual consumido.

**Fluxos alternativos / exceção:** nenhum — é uma consulta; a ausência de orçamento (pré-condição não satisfeita) é tratada na camada de apresentação como "sem orçamento definido", não como erro.

**Pós-condições:** nenhuma alteração de estado — consulta pura.

---

## UC10 — Alertar orçamento estourado

**Ator:** nenhum ator direto — caso de uso condicional, acionado via `«extend»` por UC04 (registrar despesa), nunca isolado.

**Pré-condições:** existe `Orcamento` definido para a categoria da despesa recém-registrada, no mês de referência.

**Fluxo principal:**
1. Depois que UC04 persiste a despesa, o sistema soma o total gasto na categoria no mês (mesma regra de UC09).
2. Sistema chama `Orcamento.Estourado(totalGastoNoMes)`.
3. Se `true`, o alerta viaja como campo informativo (`AlertaOrcamento`) na resposta `201` de UC04 — **não** passa pelo `INotificadorService`, porque esse mecanismo sempre vira erro `4xx` e bloquearia um lançamento que a regra manda aceitar.

**Fluxos alternativos / exceção:** não há caminho de erro — este caso de uso nunca bloqueia nada; se disparasse um `400`, seria a implementação errada da regra "orçamento avisa, não bloqueia".

**Pós-condições:** nenhuma alteração de estado adicional além da despesa já persistida por UC04 — é só um aviso na resposta.

---

## UC11 — Criar meta de economia

**Ator:** Usuário

**Pré-condições:** se vinculada a uma conta, a conta deve existir e pertencer ao usuário.

**Fluxo principal:**
1. Usuário informa nome, valor-alvo, data-alvo e, opcionalmente, a conta associada.
2. Sistema chama `MetaEconomia.Criar(idUsuario, nome, valorAlvo, dataAlvo, idConta)`.
3. Sistema persiste a meta com `Concluida = false`.

**Fluxos alternativos / exceção:**
- **E1 — dados inválidos** (valor-alvo ≤ 0, data-alvo no passado): notificador, `400 ProblemDetails`.

**Pós-condições:** nova `MetaEconomia` existe, sem aportes, disponível para UC12.

---

## UC12 — Registrar aporte em meta

**Ator:** Usuário

**Pré-condições:** meta existente e pertencente ao usuário; se o aporte estiver ligado a uma transação/transferência de saída de uma conta, essa operação já respeitou a regra de saldo da conta de origem (a meta em si **não** valida nem bloqueia saldo).

**Fluxo principal:**
1. Usuário informa o valor do aporte, a data e, se aplicável, a transação de origem (`IdTransacao`).
2. Sistema chama `MetaEconomia.RegistrarAporte(valor, data, idTransacao)`, que internamente cria um `AporteMeta`.
3. Sistema chama `MetaEconomia.VerificarConclusao()` — atualiza `Concluida` se `TotalAportado() >= ValorAlvo`.
4. Sistema persiste.

**Fluxos alternativos / exceção:**
- **E1 — valor de aporte inválido** (≤ 0): notificador, `400 ProblemDetails`. Saldo insuficiente na conta de origem, se houver, é um erro do caso de uso de transação/transferência que originou o aporte, não deste.

**Pós-condições:** novo `AporteMeta` persistido; `TotalAportado()` da meta aumentado; `Concluida = true` se o alvo foi atingido — este domínio não trava o valor nem gera efeito colateral automático ao concluir, é só um estado a exibir.

---

## UC13 — Cadastrar cartão de crédito

**Ator:** Usuário

**Pré-condições:** conta de pagamento (`IdContaPagamento`) existente e pertencente ao usuário.

**Fluxo principal:**
1. Usuário informa nome, limite total, dia de fechamento, dia de vencimento e conta de pagamento.
2. Sistema chama `Cartao.Criar(idUsuario, nome, limiteTotal, diaFechamento, diaVencimento, idContaPagamento)`.
3. Sistema persiste o cartão como `Ativo`, sem faturas ainda — a primeira fatura é aberta pela primeira compra ou pelo job de fechamento (UC20).

**Fluxos alternativos / exceção:**
- **E1 — dados inválidos** (limite ≤ 0, dia de fechamento/vencimento fora do intervalo de dias do mês): notificador, `400 ProblemDetails`.

**Pós-condições:** novo `Cartao` ativo, disponível para UC14. O cartão **não** afeta o saldo bancário diretamente nem tem saldo próprio — só limite.

---

## UC14 — Registrar compra no cartão

**Ator:** Usuário (diretamente), Sistema (via UC21 — geração de recorrência com `IdCartao` preenchido)

**Inclui, em sequência:** UC15 — Verificar limite disponível do cartão (pode reprovar a compra inteira); se aprovado, UC16 — Gerar parcelas da compra

**Pré-condições:** cartão ativo; categoria do tipo `Despesa` ativa e pertencente ao usuário.

**Fluxo principal** (diagrama de sequência 3 de `diagramas-sequencia.md`):
1. Usuário informa cartão, categoria, data, descrição, valor total e número de parcelas.
2. Sistema executa UC15 (soma parcelas futuras não pagas + `Cartao.LimiteDisponivel(...)`).
3. Se dentro do limite, `Compra.Registrar(...)` é chamado.
4. Sistema executa UC16 — gera as N parcelas, uma por fatura consecutiva.
5. Sistema persiste compra, parcelas e faturas atualizadas/abertas no mesmo `SaveChangesAsync`.

**Fluxos alternativos / exceção:**
- **E1 — valor total da compra maior que o limite disponível**: notificador acumula "Limite disponível insuficiente", nada é persistido, `400 ProblemDetails`. Vale tanto para compra à vista quanto parcelada — é o valor total, não a parcela, que é comparado ao limite.

**Pós-condições:** nova `Compra` persistida com N `Parcela`; limite disponível do cartão reduzido no valor total da compra (todas as parcelas comprometem o limite de uma vez, não uma por mês); faturas dos próximos N ciclos existem (abertas pelo próprio fluxo, se ainda não existiam) e têm seu `ValorTotal` recalculado.

---

## UC15 — Verificar limite disponível do cartão

**Ator:** nenhum ator direto — sempre acionado via `«include»` por UC14, nunca pelo usuário isoladamente.

**Pré-condições:** cartão identificado e valor total da compra que está chamando.

**Fluxo principal:**
1. Sistema soma todas as parcelas futuras ainda não pagas do cartão, em **todas** as faturas (aberta atual + fechadas não pagas + já geradas por parcelamentos anteriores) — não só a fatura aberta.
2. Sistema chama `Cartao.LimiteDisponivel(totalComprometido)` = `LimiteTotal - totalComprometido`.
3. Devolve ao caso de uso chamador se a compra cabe.

**Fluxos alternativos / exceção:** nenhum próprio — quem decide bloquear é UC14.

**Pós-condições:** nenhuma alteração de estado — consulta pura. Pagar uma fatura (UC17) libera limite referente só às parcelas daquela fatura, não o limite inteiro do cartão.

---

## UC16 — Gerar parcelas da compra

**Ator:** nenhum ator direto — sempre acionado via `«include»` por UC14, só depois que UC15 aprova.

**Pré-condições:** compra aprovada pelo limite (UC15); número de parcelas ≥ 1.

**Fluxo principal:**
1. Para cada parcela de 1 a N: sistema obtém ou abre a fatura do ciclo correspondente (`ObterOuAbrirFaturaDoCiclo`) — a 1ª parcela cai na fatura que fecha no ciclo da compra (ou no seguinte, se a compra foi feita após o dia de fechamento), a 2ª na do mês seguinte, e assim por diante.
2. Sistema chama `Parcela.Criar(idCompra, fatura.IdFatura, numero, valorDaParcela)` — o valor de cada parcela é `valorTotal / N`, com a última parcela ajustada para a soma bater exatamente com o valor total (arredondamento de centavos).
3. Sistema chama `Fatura.RecalcularValorTotal(somaParcelas)` para cada fatura afetada.

**Fluxos alternativos / exceção:**
- **E1 — a fatura do ciclo já existe e está `Fechada` ou `Paga`**: não é um caminho válido deste fluxo — indica erro de cálculo do ciclo (bug), não uma condição de negócio a tratar; uma fatura fechada nunca deveria ser o destino calculado para uma parcela nova, porque `Fatura.AceitaNovoLancamento()` só é `true` para `Aberta`.

**Pós-condições:** N `Parcela` persistidas, distribuídas em N faturas consecutivas (algumas podem ter sido abertas neste próprio fluxo); `ValorTotal` de cada fatura afetada atualizado.

---

## UC17 — Pagar fatura

**Ator:** Usuário

**Inclui:** UC04 — Registrar despesa (pagar fatura é, sob o capô, uma despesa comum na conta de pagamento do cartão — não um caso de uso próprio de lançamento, para não duplicar a regra de saldo insuficiente em dois lugares)

**Pré-condições:** fatura existente, com `Status` igual a `Aberta` (pagamento antecipado) ou `Fechada` (fluxo normal) — nunca `Paga`.

**Fluxo principal** (diagrama de sequência 5 de `diagramas-sequencia.md`):
1. Usuário aciona o pagamento de uma fatura.
2. Sistema verifica que `fatura.Status != Paga`.
3. Sistema executa UC04 sobre a conta de pagamento do cartão (`Cartao.IdContaPagamento`), com valor igual a `fatura.ValorTotal` — incluindo a verificação de saldo (UC05).
4. Se aprovado, `Fatura.RegistrarPagamento(idTransacaoPagamento)` muda o status para `Paga`.

**Fluxos alternativos / exceção:**
- **E1 — fatura já está `Paga`**: notificador, `400 ProblemDetails`, nada é feito.
- **E2 — saldo insuficiente na conta de pagamento**: mesma regra de UC05; notificador, `400 ProblemDetails`, o pagamento inteiro é abortado (nenhuma despesa é criada e a fatura continua `Aberta`/`Fechada`).

**Pós-condições:** uma `Transacao` de despesa criada na conta de pagamento; `Fatura.Status = Paga`, `IdTransacaoPagamento` preenchido; limite do cartão referente às parcelas daquela fatura volta a ficar disponível (nenhuma `Parcela` é apagada — só deixa de contar como "não paga" em UC15). Transição de estado documentada em [`diagrama-estados.md`](diagrama-estados.md).

---

## UC18 — Criar recorrência

**Ator:** Usuário

**Pré-condições:** exatamente uma de `IdConta` ou `IdCartao` deve ser informada (nunca as duas, nunca nenhuma) — é o que decide, em UC21, se a ocorrência gerada será uma transação (UC03/UC04) ou uma compra (UC14); categoria do mesmo tipo compatível com `TipoTransacao`.

**Fluxo principal:**
1. Usuário informa conta ou cartão, categoria, tipo (receita/despesa), valor, frequência (`Semanal`/`Mensal`/`Anual`), dia de geração, data de início e, opcionalmente, data de término.
2. Sistema chama `Recorrencia.Criar(idUsuario, idConta, idCartao, idCategoria, tipoTransacao, valor, frequencia, diaGeracao, dataInicio)`.
3. Sistema persiste a recorrência como `Ativa`.

**Fluxos alternativos / exceção:**
- **E1 — `IdConta` e `IdCartao` preenchidos ao mesmo tempo, ou nenhum dos dois**: notificador, `400 ProblemDetails`.

**Pós-condições:** nova `Recorrencia` ativa; a partir do próximo ciclo em que `ProximaDataGeracao(hoje)` bater com a data de execução do job, UC21 passa a gerar ocorrências automaticamente. Editar valor/categoria/dia depois de criada só afeta ocorrências futuras — as já geradas não são alteradas retroativamente.

---

## UC19 — Pausar / cancelar recorrência

**Ator:** Usuário

**Pré-condições:** recorrência existente e pertencente ao usuário.

**Fluxo principal:**
1. Usuário aciona pausar (ou reativar) a recorrência.
2. Sistema chama `Recorrencia.Pausar()` (ou `Recorrencia.Reativar()`).
3. Sistema persiste o novo estado.

**Fluxos alternativos / exceção:**
- **A1 — recorrência já `Encerrada`** (estado computado quando `DataFim` já passou, ver `diagrama-estados.md`): `Reativar()` numa recorrência pausada cuja `DataFim` já passou é zona cinzenta — não confirmado no código se o método deveria recusar ou se simplesmente não tem efeito prático (o job nunca mais vai gerar, porque `ProximaDataGeracao` continua devolvendo `null`). Confirme antes de implementar esse detalhe.

**Pós-condições:** `Recorrencia.Ativa = false` (pausada) ou `true` (reativada). Pausar/cancelar **não** apaga nem reverte transações/compras já geradas — só impede novas gerações futuras; excluir o histórico já gerado é uma ação separada e explícita sobre a transação em si.

> Zona cinzenta — não confirmada no código: o diagrama de classes ([`uml-classes.md`](uml-classes.md)) só expõe `Pausar()`/`Reativar()` em `Recorrencia`, não um método `Cancelar()` distinto. "Cancelar" no nome deste caso de uso (herdado do diagrama de casos de uso) hoje mapeia para `Pausar()` — se o produto precisar de uma diferença real entre "pausado, pode reativar" e "cancelado, definitivo", isso é uma extensão a confirmar, não implemente um `Cancelar()` novo sem alinhar antes.

---

## UC20 — Fechar fatura do ciclo

**Ator:** Sistema (job agendado — `FechamentoFaturaJob`, roda diariamente à meia-noite)

**Pré-condições:** existe pelo menos um `Cartao` ativo cujo `DiaFechamento` é o dia de hoje.

**Fluxo principal** (diagrama de sequência 4 de `diagramas-sequencia.md`):
1. Job abre um escopo de DI (é singleton, precisa resolver repositório/`DbContext` scoped).
2. Job busca todos os cartões ativos com `DiaFechamento == hoje.Day`.
3. Para cada cartão: busca a fatura `Aberta` do ciclo atual e chama `Fatura.Fechar()`.
4. Job verifica se a fatura do próximo ciclo já existe; se não, chama `Fatura.Abrir(...)` para garantir que compras feitas amanhã tenham onde cair.

**Fluxos alternativos / exceção:** este fluxo não tem caminho de erro de negócio (não há "fechamento inválido" a rejeitar) — falha de persistência propaga como exceção não tratada no nível de job, fora do padrão `INotificadorService` (que é para requisições HTTP).

**Pós-condições:** a fatura do ciclo que fechou passa a `Fechada` (transição documentada em `diagrama-estados.md`) e se torna imutável — nenhuma `Parcela` nova entra nela, mesmo que a data da compra seja retroativa; a fatura do próximo ciclo existe e está `Aberta`.

---

## UC21 — Gerar ocorrência de recorrência

**Ator:** Sistema (job agendado — `RecorrenciaJob`, roda diariamente à meia-noite)

**Inclui, como alternativas (nunca as três juntas):** UC03 — Registrar receita, UC04 — Registrar despesa, ou UC14 — Registrar compra no cartão — qual delas dispara depende de como a `Recorrencia` foi configurada em UC18 (`IdConta` preenchido → UC03 ou UC04, conforme `TipoTransacao`; `IdCartao` preenchido → UC14).

**Pré-condições:** existe pelo menos uma `Recorrencia` com `Ativa = true` cuja `ProximaDataGeracao(hoje)` é igual a hoje.

**Fluxo principal** (diagrama de sequência 6 de `diagramas-sequencia.md`):
1. Job abre escopo de DI e busca todas as recorrências ativas.
2. Para cada uma, calcula `Recorrencia.ProximaDataGeracao(hoje)`.
3. Se bate com hoje: se `IdConta` preenchido, chama o mesmo fluxo de UC03/UC04 (`TransacaoService`); se `IdCartao` preenchido, chama o mesmo fluxo de UC14 (`CompraService`) — **exatamente o mesmo caminho** que um lançamento manual, incluindo validação de saldo/limite. Uma recorrência não tem passe livre para violar o mesmo invariante que bloquearia um lançamento humano.

**Fluxos alternativos / exceção:**
- **A1 — geração bloqueada por saldo/limite insuficiente**: a ocorrência não é criada; o job registra a falha em log para revisão do usuário, mas não interrompe o processamento das demais recorrências do lote.

**Pós-condições:** nova `Transacao` ou `Compra` persistida (mesmas pós-condições de UC03/UC04/UC14, conforme o caminho tomado) **ou**, se bloqueada, nenhuma alteração de estado além do registro em log. A `Recorrencia` em si não é alterada por este caso de uso — sua próxima data de geração é recalculada a partir de `DiaGeracao`/`Frequencia` na próxima execução do job, não é um campo gravado que precisa de update aqui.

---

## Fora do escopo

Os mesmos itens já listados no `README.md` deste diretório (múltiplas moedas, conta compartilhada entre mais de um usuário, integração com extrato bancário, teto configurável de cheque especial, taxa de transferência, estorno parcial de compra parcelada já paga parcialmente) não têm caso de uso especificado aqui — são extensões a confirmar antes de modelar, não funcionalidades já decididas.

Este documento também deixou explícitas duas zonas cinzentas próprias, além das do README: inativar conta com saldo diferente de zero (UC02) e o significado exato de "cancelar" recorrência versus "pausar" (UC19).
