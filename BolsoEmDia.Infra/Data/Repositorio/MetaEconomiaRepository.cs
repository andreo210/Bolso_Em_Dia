using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class MetaEconomiaRepository : RepositorioGlobal<MetaEconomia>, IMetaEconomiaRepository
    {
        public MetaEconomiaRepository(AppDbContext context) : base(context) { }
    }
}
