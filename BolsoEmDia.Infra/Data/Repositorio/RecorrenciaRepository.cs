using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Infra.Data.Repositorio
{
    public class RecorrenciaRepository : RepositorioGlobal<Recorrencia>, IRecorrenciaRepository
    {
        public RecorrenciaRepository(AppDbContext context) : base(context) { }
    }
}
