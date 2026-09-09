using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class TransferenciaRepository : RepositorioGlobal<Transferencia>, ITransferenciaRepository
    {
        public TransferenciaRepository(AppDbContext context) : base(context) { }
    }
}
