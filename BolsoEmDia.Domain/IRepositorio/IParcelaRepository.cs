using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Domain.IRepositorio
{
    public interface IParcelaRepository : IRepositorioGlobal<Parcela>
    {
        /// <summary>
        /// Soma de todas as parcelas de faturas não pagas (aberta + fechadas não pagas) de um cartão —
        /// é o que consome o limite disponível (ver Cartao.LimiteDisponivel). Precisa somar cruzando com
        /// Fatura.Status, o que não cabe em filtro/incluir do repositório genérico.
        /// </summary>
        Task<decimal> ObterTotalParcelasNaoPagasAsync(int idCartao, CancellationToken ct = default);
    }
}
