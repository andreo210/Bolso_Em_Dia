using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Tests.Fakes
{
    /*
     * As interfaces concretas de repositório não declaram nada além de IRepositorioGlobal<T>,
     * então cada fake tipado é só a amarração do genérico com a interface que o serviço pede.
     * Se algum dia uma interface ganhar um método próprio, é aqui que ele entra — como em
     * TransacaoRepositoryFake abaixo.
     */

    public class ContaRepositoryFake : RepositorioFake<Conta>, IContaRepository
    {
        public ContaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class CategoriaRepositoryFake : RepositorioFake<Categoria>, ICategoriaRepository
    {
        public CategoriaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class OrcamentoRepositoryFake : RepositorioFake<Orcamento>, IOrcamentoRepository
    {
        public OrcamentoRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class MetaEconomiaRepositoryFake : RepositorioFake<MetaEconomia>, IMetaEconomiaRepository
    {
        public MetaEconomiaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class TransferenciaRepositoryFake : RepositorioFake<Transferencia>, ITransferenciaRepository
    {
        public TransferenciaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class CartaoRepositoryFake : RepositorioFake<Cartao>, ICartaoRepository
    {
        public CartaoRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }

        /// <summary>
        /// Propaga faturas novas (abertas via Cartao.AbrirFatura) para a própria tabela —
        /// no banco de verdade isso é a cascata Cartao.Faturas do EF; sem isso, ParcelaRepositoryFake
        /// nunca enxergaria a fatura ao somar parcelas não pagas em CompraService (ver UC15).
        /// </summary>
        public override Task<int> SalvarAsync(CancellationToken ct = default)
        {
            var tabelaFaturas = Armazem.Tabela<Fatura>();

            foreach (var cartao in Tabela)
            foreach (var fatura in cartao.Faturas)
                if (!tabelaFaturas.Contains(fatura))
                    Armazem.Semear(fatura);

            return base.SalvarAsync(ct);
        }
    }

    public class CompraRepositoryFake : RepositorioFake<Compra>, ICompraRepository
    {
        public CompraRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class FaturaRepositoryFake : RepositorioFake<Fatura>, IFaturaRepository
    {
        public FaturaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class ParcelaRepositoryFake : RepositorioFake<Parcela>, IParcelaRepository
    {
        public ParcelaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }

        /// <summary>Mesma junção Parcela↔Fatura que ParcelaRepository faz no banco (ver UC15).</summary>
        public Task<decimal> ObterTotalParcelasNaoPagasAsync(int idCartao, CancellationToken ct = default)
        {
            var total = Armazem.Tabela<Fatura>()
                .Where(f => f.IdCartao == idCartao && f.Status != StatusFatura.Paga)
                .Join(Armazem.Tabela<Parcela>(), f => f.IdFatura, p => p.IdFatura, (f, p) => p.Valor)
                .Sum();

            return Task.FromResult(total);
        }
    }

    public class RecorrenciaRepositoryFake : RepositorioFake<Recorrencia>, IRecorrenciaRepository
    {
        public RecorrenciaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
    }

    public class TransacaoRepositoryFake : RepositorioFake<Transacao>, ITransacaoRepository
    {
        public TransacaoRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }

        /// <summary>Mesma regra de sinal que ContaService.ObterSaldoAsync aplica hoje "na mão".</summary>
        public Task<decimal> ObterSaldoAsync(int idConta, DateTime dataReferencia, CancellationToken ct = default)
        {
            var saldo = Armazem.Tabela<Transacao>()
                .Where(t => t.IdConta == idConta && t.Data.Date <= dataReferencia.Date)
                .Sum(t => t.Tipo is TipoTransacao.Despesa or TipoTransacao.TransferenciaSaida ? -t.Valor : t.Valor);

            return Task.FromResult(saldo);
        }

        public Task<decimal> ObterTotalDespesasNoPeriodoAsync(
            IReadOnlyCollection<int> idsCategoria, DateTime inicio, DateTime fim, CancellationToken ct = default)
        {
            var total = Armazem.Tabela<Transacao>()
                .Where(t => t.Tipo == TipoTransacao.Despesa
                    && t.IdCategoria.HasValue && idsCategoria.Contains(t.IdCategoria.Value)
                    && t.Data.Date >= inicio.Date && t.Data.Date <= fim.Date)
                .Sum(t => t.Valor);

            return Task.FromResult(total);
        }
    }
}
