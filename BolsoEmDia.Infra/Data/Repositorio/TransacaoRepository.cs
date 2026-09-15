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

        public async Task<IReadOnlyList<(int IdCategoria, decimal Total)>> ObterGastosPorCategoriaAsync(
            string idUsuario, DateTime inicio, DateTime fim, CancellationToken ct = default)
        {
            var resultado = await DbSet
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario && t.Tipo == TipoTransacao.Despesa
                    && t.IdCategoria != null && t.Data >= inicio && t.Data <= fim)
                .GroupBy(t => t.IdCategoria!.Value)
                .Select(g => new { IdCategoria = g.Key, Total = g.Sum(t => t.Valor) })
                .ToListAsync(ct);

            return resultado.Select(r => (r.IdCategoria, r.Total)).ToList();
        }

        public async Task<IReadOnlyList<(int Ano, int Mes, decimal TotalReceitas, decimal TotalDespesas)>> ObterEvolucaoMensalAsync(
            string idUsuario, DateTime inicio, DateTime fim, CancellationToken ct = default)
        {
            var resultado = await DbSet
                .AsNoTracking()
                .Where(t => t.IdUsuario == idUsuario
                    && (t.Tipo == TipoTransacao.Receita || t.Tipo == TipoTransacao.Despesa)
                    && t.Data >= inicio && t.Data <= fim)
                .GroupBy(t => new { t.Data.Year, t.Data.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalReceitas = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
                    TotalDespesas = g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
                })
                .ToListAsync(ct);

            return resultado.Select(r => (r.Year, r.Month, r.TotalReceitas, r.TotalDespesas)).ToList();
        }
    }
}
