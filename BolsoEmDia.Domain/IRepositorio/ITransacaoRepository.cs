using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Domain.IRepositorio
{
    public interface ITransacaoRepository : IRepositorioGlobal<Transacao>
    {
        /// <summary>
        /// Saldo de uma conta até <paramref name="dataReferencia"/>: soma de Receita/TransferenciaEntrada
        /// menos Despesa/TransferenciaSaida das transações efetivadas (Data ≤ dataReferencia). Não cabe em
        /// filtro/incluir porque depende de somar com sinal por Tipo, não só filtrar.
        /// </summary>
        Task<decimal> ObterSaldoAsync(int idConta, DateTime dataReferencia, CancellationToken ct = default);

        /// <summary>
        /// Soma das despesas efetivadas de um conjunto de categorias (categoria + subcategorias,
        /// resolvidas por quem chama) num período — usado no progresso de Orcamento. Agregação por soma
        /// não é algo que filtro/ordenarPor/incluir resolvam.
        /// </summary>
        Task<decimal> ObterTotalDespesasNoPeriodoAsync(
            IReadOnlyCollection<int> idsCategoria, DateTime inicio, DateTime fim, CancellationToken ct = default);

        /// <summary>
        /// Soma das despesas do usuário num período, agrupada por categoria como a transação foi lançada
        /// (sem enrolar subcategoria no pai) — usada no gráfico de gastos por categoria do dashboard (UC22).
        /// </summary>
        Task<IReadOnlyList<(int IdCategoria, decimal Total)>> ObterGastosPorCategoriaAsync(
            string idUsuario, DateTime inicio, DateTime fim, CancellationToken ct = default);

        /// <summary>
        /// Soma de receitas e despesas do usuário por mês num período — usada na evolução mensal do
        /// dashboard (UC22). Ignora TransferenciaEntrada/TransferenciaSaida: são movimento entre contas
        /// do próprio usuário, não renda/gasto real.
        /// </summary>
        Task<IReadOnlyList<(int Ano, int Mes, decimal TotalReceitas, decimal TotalDespesas)>> ObterEvolucaoMensalAsync(
            string idUsuario, DateTime inicio, DateTime fim, CancellationToken ct = default);
    }
}
