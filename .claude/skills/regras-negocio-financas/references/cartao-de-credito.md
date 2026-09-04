# Cartão de crédito, fatura, parcelamento e recorrência

## Cartão não é uma conta

Um cartão de crédito tem **limite total**, um **dia de fechamento** e um **dia de vencimento** — não tem saldo próprio. Uma compra no cartão nunca debita diretamente uma conta bancária. O único momento em que dinheiro sai de uma conta é o **pagamento da fatura**, que gera uma despesa normal na conta bancária vinculada ao cartão.

## Ciclo da fatura

Cada cartão define `dia_fechamento` e `dia_vencimento` (ex.: fecha dia 5, vence dia 12).

- Uma compra feita **até** o dia de fechamento do ciclo corrente entra na fatura que fecha nesse ciclo.
- Uma compra feita **depois** do dia de fechamento entra na fatura do ciclo seguinte (a fatura "vira" no dia seguinte ao fechamento).
- Cada fatura tem um estado: **aberta** (ainda recebendo lançamentos, ainda não fechou) → **fechada** (fechou, valor definido, aguardando pagamento até o vencimento) → **paga** (o usuário registrou o pagamento).
- Uma fatura **fechada é imutável**: nenhuma compra nova entra nela, mesmo que a data da compra seja retroativa ao período dela — nesse caso a compra vai para a fatura aberta seguinte, não para a fechada.
- O pagamento da fatura gera uma transação de despesa na conta bancária vinculada, na data em que o usuário registra o pagamento (ou, se o produto quiser automação, na data de vencimento — mas isso é uma extensão, não assuma sem confirmar). Pagar a fatura libera o limite correspondente ao valor pago.

## Limite disponível

Esta é a regra mais fácil de implementar errado: **limite disponível não é "limite total menos a fatura aberta atual"**. É:

```
limite_disponível = limite_total − soma de TODAS as parcelas futuras ainda não pagas,
                     em todas as faturas (aberta atual + fechadas não pagas + futuras já geradas por parcelamento)
```

Ou seja, se o usuário compra algo em 10x, as 10 parcelas comprometem o limite **inteiro no momento da compra**, não uma parcela por vez ao longo dos meses. Isso é o que evita que o usuário "estoure" o cartão comprando parcelado sem perceber que já comprometeu meses futuros.

- Uma nova compra é **bloqueada** se o valor total dela (soma de todas as parcelas, se for parcelada) exceder o limite disponível calculado acima. Isso vale tanto para compra à vista quanto parcelada.
- Pagar uma fatura libera o limite referente às parcelas daquela fatura especificamente (não o limite inteiro).

## Parcelamento

- Uma compra parcelada em N vezes gera N transações (parcelas) de valor `valor_total / N` (trate arredondamento de centavos ajustando a última parcela para que a soma bata exatamente com o valor total da compra).
- Cada parcela é atribuída a uma fatura consecutiva: a 1ª parcela na fatura que fecha no ciclo da compra (ou no ciclo seguinte se comprada após o fechamento, como em qualquer compra), a 2ª na fatura do mês seguinte, e assim por diante.
- Todas as N parcelas contam contra o limite disponível assim que a compra é feita (ver seção anterior) — não só a parcela da fatura aberta.
- Cancelar/estornar uma compra parcelada antes de qualquer parcela ser paga remove todas as parcelas futuras e libera o limite total dela. Se parte das parcelas já foi paga (faturas já pagas), o estorno das parcelas restantes é o caso comum; parcelas já pagas normalmente geram uma transação de estorno na conta, mas isso é uma extensão — confirme o comportamento esperado antes de implementar estorno parcial.

## Recorrência / assinaturas

Recorrência é um conceito mais amplo que cartão — vale para qualquer transação que se repete automaticamente (salário recebido todo mês, aluguel, assinatura de streaming no cartão, etc.).

- Uma recorrência é um **modelo**: valor, conta (ou cartão), categoria, frequência (semanal, mensal, anual), dia do lançamento, data de início e data de término opcional.
- A cada ciclo, o sistema gera automaticamente uma transação real a partir do modelo, na data correspondente. Essas transações geradas seguem todas as regras normais de transação (ou de compra no cartão, se a recorrência for em um cartão) — inclusive bloqueio por saldo insuficiente ou limite insuficiente, se aplicável.
- **Editar uma recorrência** (valor, categoria, dia) afeta só as ocorrências que ainda serão geradas a partir dali. Transações já geradas no passado por aquele modelo não são retroativamente alteradas.
- **Cancelar/pausar uma recorrência** impede novas gerações futuras, mas não apaga nem reverte as transações já geradas. Excluir essas transações passadas, se necessário, é uma ação separada e explícita do usuário sobre a transação em si, não um efeito colateral de cancelar o modelo.
- Se a data de geração cai em um dia que não existe no mês (ex.: recorrência todo dia 31 em um mês com 30 dias), ajuste para o último dia do mês — não pule a geração daquele ciclo.
