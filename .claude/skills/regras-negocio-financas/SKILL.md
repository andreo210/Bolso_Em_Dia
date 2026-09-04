---
name: regras-negocio-financas
description: Regras de negócio do sistema de organização financeira pessoal — contas (corrente, poupança, carteira, investimento), transações e saldo, categorias e orçamentos mensais, cartão de crédito (fatura, fechamento, vencimento, limite, parcelamento) e transações recorrentes/assinaturas. Use sempre que for escrever, revisar ou discutir código desse sistema financeiro: criar/alterar entidade, serviço ou validação de conta, transação, transferência, categoria, orçamento, cartão, fatura, parcela ou recorrência; decidir se um lançamento deve ser bloqueado ou apenas alertado; calcular saldo, limite disponível ou progresso de orçamento; ou modelar o ciclo de uma fatura de cartão. Vale também para perguntas do tipo "pode a conta ficar negativa aqui", "isso entra na fatura de qual mês", "o orçamento estourado bloqueia o lançamento" e "como o limite do cartão é consumido pelo parcelamento".
---

# Regras de negócio — organização financeira pessoal

Este documento descreve o **domínio**, não uma arquitetura de código. As regras aqui valem independente da stack usada para implementar (API, front, script de importação, etc.) — se o repositório também tiver uma skill de arquitetura (ex.: `arquitetura-api`, `arquitetura-front`), combine as duas: a arquitetura diz *onde* o código mora, esta skill diz *o que* o código precisa garantir.

Moeda: sistema de moeda única (BRL). Não há conversão de câmbio nem contas multimoeda — se isso for pedido, é uma mudança de escopo, avise antes de implementar.

## Quando consultar cada referência

| Se a tarefa envolve... | Leia |
|---|---|
| Conta bancária/carteira, saldo, lançamento de receita/despesa, transferência entre contas | `references/contas-e-transacoes.md` |
| Categoria de gasto/receita, orçamento mensal, meta de economia | `references/categorias-e-orcamentos.md` |
| Cartão de crédito, fatura, fechamento/vencimento, limite, parcelamento, recorrência/assinatura | `references/cartao-de-credito.md` |

Não é preciso ler os três de uma vez — abra só o que a tarefa em mãos toca. Se a tarefa cruza dois domínios (ex.: pagamento de fatura que gera uma despesa na conta corrente), leia os dois.

## Entidades principais

- **Conta**: corrente, poupança, carteira (dinheiro) ou investimento. Tem saldo. Cartão de crédito **não** é um tipo de conta — é uma entidade à parte que não tem saldo próprio, só limite (ver `cartao-de-credito.md`).
- **Transação**: receita, despesa ou transferência. Sempre datada, sempre com valor positivo, sempre ligada a uma conta (ou duas, no caso de transferência).
- **Categoria**: classifica receitas e despesas para fins de orçamento e relatório. Transferência não tem categoria.
- **Orçamento**: teto mensal de gasto por categoria de despesa.
- **Cartão**: limite total, dia de fechamento, dia de vencimento. Gera faturas mensais.
- **Fatura**: agrupa as compras de um cartão em um ciclo; tem estado aberta/fechada/paga.
- **Recorrência**: modelo que gera transações futuras automaticamente em um ritmo fixo (assinatura, salário, aluguel).

## Invariantes críticos (não violar sem confirmar com o usuário)

1. Saldo de uma conta = soma de todas as transações já efetivadas até a data atual. Transações agendadas para o futuro **não** entram no saldo corrente.
2. Conta corrente pode ficar negativa (cheque especial) — não bloquear. Carteira, poupança e investimento **não podem** ficar negativas — bloquear o lançamento que deixaria o saldo abaixo de zero.
3. Cartão de crédito nunca afeta o saldo de uma conta bancária diretamente. Só o **pagamento da fatura** gera uma despesa na conta vinculada.
4. Limite disponível do cartão = limite total − soma de **todas** as parcelas futuras ainda não pagas (não só a fatura do mês corrente). Uma compra é bloqueada se o valor total dela (todas as parcelas) exceder o limite disponível.
5. Uma fatura fechada é imutável: nenhum lançamento novo entra nela, mesmo que a data da compra seja retroativa ao ciclo dela.
6. Orçamento estourado gera alerta, nunca bloqueia o lançamento. O consumo do orçamento reinicia zerado a cada mês — não há acúmulo (rollover) de saldo não usado.
7. Transferência entre contas sempre gera duas pernas atômicas (saída da origem + entrada no destino), nunca tem categoria e nunca é contada como receita ou despesa em relatórios por categoria.
8. Editar ou cancelar uma recorrência afeta só as ocorrências futuras. Lançamentos já gerados no passado nunca são alterados retroativamente por uma mudança no modelo de recorrência.

## Checklist ao implementar uma funcionalidade

Antes de considerar pronta uma mudança neste domínio, confira:

- [ ] O cálculo de saldo/limite envolvido respeita os invariantes acima?
- [ ] A regra é "bloquear" ou "alertar"? Confirme qual — errar isso é o erro mais comum aqui (orçamento sempre alerta, saldo de carteira/poupança sempre bloqueia, cartão sem limite sempre bloqueia).
- [ ] Se a mudança toca cartão de crédito, ela considera parcelas futuras já comprometidas contra o limite, não só a fatura aberta?
- [ ] Se a mudança toca transferência, as duas pernas são criadas/desfeitas juntas (nunca uma órfã)?
- [ ] Se a mudança toca recorrência, ela deixa claro que só afeta ocorrências futuras?

Se alguma dessas respostas depende de uma decisão de produto ainda não tomada, pergunte ao usuário em vez de assumir — essas regras têm efeito direto sobre dinheiro real do usuário do sistema.
