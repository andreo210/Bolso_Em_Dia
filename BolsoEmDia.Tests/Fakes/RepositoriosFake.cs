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

    public class TransferenciaRepositoryFake : RepositorioFake<Transferencia>, ITransferenciaRepository
    {
        public TransferenciaRepositoryFake(ArmazemFake? armazem = null) : base(armazem) { }
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
