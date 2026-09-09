using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class OrcamentoRepository : RepositorioGlobal<Orcamento>, IOrcamentoRepository
    {
        public OrcamentoRepository(AppDbContext context) : base(context) { }
    }
}
