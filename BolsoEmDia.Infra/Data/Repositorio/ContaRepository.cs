using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class ContaRepository : RepositorioGlobal<Conta>, IContaRepository
    {
        public ContaRepository(AppDbContext context) : base(context) { }
    }
}
