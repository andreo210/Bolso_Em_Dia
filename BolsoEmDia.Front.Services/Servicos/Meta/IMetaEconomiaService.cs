using BolsoEmDia.Front.Models.Request.Meta;
using BolsoEmDia.Front.Models.Response.Meta;

namespace BolsoEmDia.Front.Services.Servicos.Meta
{
    public interface IMetaEconomiaService
    {
        Task<List<MetaEconomiaResponse>?> ObterTodos(CancellationToken ct = default);
        Task<MetaEconomiaResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<List<AporteMetaResponse>?> ObterAportes(int id, CancellationToken ct = default);
        Task<MetaEconomiaResponse?> Criar(CriarMetaEconomiaRequest request, CancellationToken ct = default);
        Task<AporteMetaResponse?> RegistrarAporte(int id, RegistrarAporteMetaRequest request, CancellationToken ct = default);
    }
}
