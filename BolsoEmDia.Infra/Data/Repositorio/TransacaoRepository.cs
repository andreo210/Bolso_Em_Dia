using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class TransacaoRepository : RepositorioGlobal<Transacao>, ITransacaoRepository
    {
        public TransacaoRepository(AppDbContext context) : base(context) { }

        public async Task<decimal> ObterSaldoAsync(int idConta, DateTime dataReferencia, CancellationToken ct = default)
        {
            return await DbSet
                .AsNoTracking()
                .Where(t => t.IdConta == idConta && t.Data <= dataReferencia)
                .SumAsync(t => t.Tipo == TipoTransacao.Receita || t.Tipo == TipoTransacao.TransferenciaEntrada
                    ? t.Valor
                    : -t.Valor, ct);
        }

        public async Task<decimal> ObterTotalDespesasNoPeriodoAsync(
            IReadOnlyCollection<int> idsCategoria, DateTime inicio, DateTime fim, CancellationToken ct = default)
        {
            return await DbSet
                .AsNoTracking()
                .Where(t => t.Tipo == TipoTransacao.Despesa
                    && t.IdCategoria != null && idsCategoria.Contains(t.IdCategoria.Value)
                    && t.Data >= inicio && t.Data <= fim)
                .SumAsync(t => t.Valor, ct);
        }
    }
}
