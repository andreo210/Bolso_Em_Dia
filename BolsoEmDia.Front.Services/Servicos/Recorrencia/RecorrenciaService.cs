using BolsoEmDia.Front.Models.Request.Recorrencia;
using BolsoEmDia.Front.Models.Response.Recorrencia;

namespace BolsoEmDia.Front.Services.Servicos.Recorrencia
{
    public class RecorrenciaService : IRecorrenciaService
    {
        private const string RotaBase = "api/v1/recorrencias";
        private readonly IApiHttpService _api;

        public RecorrenciaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<RecorrenciaResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<RecorrenciaResponse>>(RotaBase, ct);

        public Task<RecorrenciaResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<RecorrenciaResponse>($"{RotaBase}/{id}", ct);

        public async Task<RecorrenciaResponse?> Inserir(CriarRecorrenciaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<RecorrenciaResponse, CriarRecorrenciaRequest>(RotaBase, request, ct);
            return resposta;
        }

        public Task<bool> Pausar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/pausar", new object(), ct);

        public Task<bool> Reativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/reativar", new object(), ct);
    }
}
