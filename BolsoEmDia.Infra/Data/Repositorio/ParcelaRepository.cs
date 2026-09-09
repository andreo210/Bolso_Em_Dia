using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class ParcelaRepository : RepositorioGlobal<Parcela>, IParcelaRepository
    {
        public ParcelaRepository(AppDbContext context) : base(context) { }

        public async Task<decimal> ObterTotalParcelasNaoPagasAsync(int idCartao, CancellationToken ct = default)
        {
            return await DbSet
                .AsNoTracking()
                .Join(Context.Set<Fatura>().AsNoTracking(),
                    parcela => parcela.IdFatura,
                    fatura => fatura.IdFatura,
                    (parcela, fatura) => new { parcela.Valor, fatura.IdCartao, fatura.Status })
                .Where(x => x.IdCartao == idCartao && x.Status != StatusFatura.Paga)
                .SumAsync(x => x.Valor, ct);
        }
    }
}
