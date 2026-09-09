using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class CompraRepository : RepositorioGlobal<Compra>, ICompraRepository
    {
        public CompraRepository(AppDbContext context) : base(context) { }
    }
}
