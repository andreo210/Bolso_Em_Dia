# Contas e transações

## Tipos de conta

- **Corrente**: pode ficar negativa (cheque especial). Não há limite de cheque especial modelado por padrão — se o produto precisar de um teto para o negativo, isso é uma extensão, confirme com o usuário antes de assumir um valor.
- **Poupança**: não pode ficar negativa.
- **Carteira (dinheiro em espécie)**: não pode ficar negativa.
- **Investimento**: não pode ficar negativa. Depósitos e resgates são tratados como transações normais (receita/despesa) na conta de investimento; rendimento também é uma receita lançada nela. Este domínio não modela cálculo de rentabilidade — só o saldo por movimentação registrada.

Cartão de crédito **não é uma conta**. Ver `cartao-de-credito.md`.

## Regra de saldo negativo

Antes de gravar uma despesa (ou a perna de saída de uma transferência), verifique o tipo da conta de origem:

- Corrente → sempre permite, mesmo que o saldo resultante seja negativo. Não é necessário alertar, mas é aceitável mostrar o saldo negativo ao usuário na interface.
- Poupança, carteira, investimento → calcule o saldo resultante; se for menor que zero, **bloqueie o lançamento** com uma mensagem clara de saldo insuficiente. Não arredonde nem tolere pequenas diferenças de centavos — a comparação é exata.

## Transação (receita/despesa)

Campos obrigatórios:
- Data (não precisa ser hoje — pode ser passada ou futura/agendada)
- Valor, sempre armazenado como positivo — o sinal (receita soma, despesa subtrai) vem do **tipo** da transação, nunca do sinal do valor
- Conta
- Categoria (receita ou despesa — a categoria deve ser do mesmo tipo da transação; despesa não pode usar categoria de receita e vice-versa)

Campo opcional: descrição/observação.

### Data futura e saldo

Uma transação com data futura à data atual do sistema é uma transação **agendada/prevista**: ela existe no sistema mas não entra no cálculo do saldo atual da conta até a data dela chegar. Ao listar "saldo atual", filtre por `data <= hoje`. Ao mostrar "saldo previsto" ou projeção, inclua as futuras. Não confunda os dois — um saldo atual que soma lançamentos futuros é o bug mais comum nesse cálculo.

### Edição e exclusão

Editar valor, data ou conta de uma transação já existente deve recalcular o saldo afetado — não há "saldo" armazenado de forma independente e cacheada sem trigger de recálculo; trate o saldo sempre como derivado (soma das transações), não como um campo que se atualiza por soma incremental sujeita a divergir do real. Se por performance for necessário cachear, o cache precisa ser invalidado/recalculado nas mesmas operações que criam, editam ou excluem transações daquela conta.

Excluir uma transação remove seu efeito do saldo. Se a transação fizer parte de uma transferência ou de uma fatura de cartão paga, ver as seções específicas — exclusão isolada de uma perna não é permitida (ver Transferência abaixo).

## Transferência entre contas

Uma transferência **não é** uma receita na conta de destino nem uma despesa na conta de origem lançada separadamente — é uma operação atômica com duas pernas:

- Perna de saída: debita o valor da conta de origem.
- Perna de entrada: credita o mesmo valor na conta de destino.

Regras:
- Nunca tem categoria. Se o sistema exige categoria em toda transação, trate transferência como um tipo à parte, não como despesa/receita "sem categoria".
- O valor é sempre positivo e igual nas duas pernas — este domínio não modela taxas de transferência; se o produto precisar disso, é uma extensão a confirmar.
- A regra de saldo negativo (acima) se aplica à perna de saída como se fosse uma despesa: se a conta de origem não pode negativar e o saldo ficaria negativo, bloqueie a transferência inteira (nenhuma das duas pernas é criada).
- As duas pernas são criadas e excluídas juntas. Não existe "metade de uma transferência" no banco de dados — se a implementação usa duas linhas de transação, elas precisam estar amarradas por uma referência comum (ex.: um `transferencia_id`) para que editar/excluir uma sempre edite/exclua a outra.
- Transferências não entram em relatórios de "receitas por categoria" nem "despesas por categoria" — elas são movimentação entre contas do próprio usuário, não entrada/saída real de dinheiro do sistema como um todo.

## Conciliação

Este domínio não assume integração automática com extrato bancário (open finance, importação de OFX, etc.) como regra padrão. Se a tarefa envolver isso, trate como extensão e alinhe o comportamento esperado (ex.: como decidir se uma transação importada é duplicata de uma já lançada manualmente) antes de implementar.
