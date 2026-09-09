using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class CategoriaRepository : RepositorioGlobal<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context) { }
    }
}
