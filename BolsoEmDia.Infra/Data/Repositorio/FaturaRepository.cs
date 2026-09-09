using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class FaturaRepository : RepositorioGlobal<Fatura>, IFaturaRepository
    {
        public FaturaRepository(AppDbContext context) : base(context) { }
    }
}
