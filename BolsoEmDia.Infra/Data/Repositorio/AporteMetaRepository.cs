using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class AporteMetaRepository : RepositorioGlobal<AporteMeta>, IAporteMetaRepository
    {
        public AporteMetaRepository(AppDbContext context) : base(context) { }
    }
}
