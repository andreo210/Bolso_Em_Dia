using BolsoEmDia.Front.Models.Request.Meta;
using BolsoEmDia.Front.Models.Response.Meta;

namespace BolsoEmDia.Front.Services.Servicos.Meta
{
    public class MetaEconomiaService : IMetaEconomiaService
    {
        private const string RotaBase = "api/v1/metas";
        private readonly IApiHttpService _api;

        public MetaEconomiaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<MetaEconomiaResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<MetaEconomiaResponse>>(RotaBase, ct);

        public Task<MetaEconomiaResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<MetaEconomiaResponse>($"{RotaBase}/{id}", ct);

        public Task<List<AporteMetaResponse>?> ObterAportes(int id, CancellationToken ct = default) =>
            _api.GetAsync<List<AporteMetaResponse>>($"{RotaBase}/{id}/aportes", ct);

        public async Task<MetaEconomiaResponse?> Criar(CriarMetaEconomiaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<MetaEconomiaResponse, CriarMetaEconomiaRequest>(RotaBase, request, ct);
            return resposta;
        }

        public async Task<AporteMetaResponse?> RegistrarAporte(int id, RegistrarAporteMetaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<AporteMetaResponse, RegistrarAporteMetaRequest>($"{RotaBase}/{id}/aportes", request, ct);
            return resposta;
        }
    }
}
