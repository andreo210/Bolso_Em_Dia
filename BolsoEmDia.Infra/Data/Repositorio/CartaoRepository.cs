using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class CartaoRepository : RepositorioGlobal<Cartao>, ICartaoRepository
    {
        public CartaoRepository(AppDbContext context) : base(context) { }
    }
}
