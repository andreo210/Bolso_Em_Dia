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
    }
}
