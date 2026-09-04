# Categorias e orçamentos

## Categorias

- Toda categoria pertence a exatamente um tipo: **receita** ou **despesa**. Uma transação só pode usar uma categoria do mesmo tipo que ela.
- Categorias podem ter subcategorias (hierarquia de um nível: categoria → subcategorias). Orçamento e relatórios são definidos por categoria; se uma subcategoria existir, o gasto nela conta tanto para o total da subcategoria quanto para o total da categoria-mãe.
- Categorias padrão (ex.: Alimentação, Transporte, Moradia, Lazer, Salário) podem ser sugeridas na criação de uma conta/usuário novo, mas o usuário deve poder criar, renomear e excluir as suas próprias. Excluir uma categoria que já tem transações lançadas não deve apagar o histórico — trate como uma restrição (bloquear exclusão) ou reatribua as transações a "Outros", mas nunca perca o valor/data da transação.

## Orçamento mensal por categoria

- Um orçamento é um teto de gasto definido por categoria de despesa, por mês (ex.: R$ 800 em Alimentação em setembro/2026).
- Categorias de receita não têm orçamento — orçamento é sempre sobre despesa.
- **Ultrapassar o orçamento gera alerta, nunca bloqueia o lançamento.** O usuário sempre pode registrar a despesa; o sistema só sinaliza que ela estourou o teto (ex.: indicador visual, notificação). Não implemente uma validação que impeça salvar a transação por causa de orçamento — isso é uma regra de bloqueio que este domínio explicitamente não tem.
- **Sem rollover**: o consumo do orçamento de uma categoria reinicia zerado a cada mês. Se o usuário gastou menos que o teto em agosto, essa sobra não aumenta o teto de setembro. Se um dia o produto quiser rollover, é uma mudança de regra a confirmar — não implemente por padrão.
- Progresso do orçamento de uma categoria em um mês = soma das despesas daquela categoria com data dentro do mês (incluindo subcategorias, se houver) dividida pelo valor do orçamento definido para aquele mês. Use `data <= hoje` da mesma forma que o cálculo de saldo (transações futuras/agendadas não contam no progresso do orçamento até a data chegar).
- Se não houver orçamento definido para uma categoria em um mês, não há "progresso" a calcular — trate como ausência de orçamento, não como orçamento zero (o que faria qualquer gasto parecer 100%+ estourado).

## Metas de economia

Metas de economia (ex.: "juntar R$ 5.000 até dezembro") são um conceito separado de orçamento: orçamento é um teto de gasto por categoria; meta é um alvo de acúmulo, geralmente ligado a uma conta ou a um valor reservado, não a uma categoria de despesa.

- Progresso de uma meta = soma dos aportes (transações ou transferências marcadas como destinadas àquela meta) até a data atual, comparado ao valor-alvo.
- Uma meta não bloqueia nem valida saldo — é só acompanhamento. Se o usuário quiser aportar mais do que sua conta permite, a regra de saldo negativo da conta de origem (ver `contas-e-transacoes.md`) é quem decide se bloqueia, não a meta em si.
- Este domínio não define automaticamente uma data de "conclusão" da meta com efeitos colaterais (ex.: travar o valor) — atingir o alvo é só um estado a exibir, a menos que o produto peça o contrário.
