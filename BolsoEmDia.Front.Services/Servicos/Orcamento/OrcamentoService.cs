using System.Globalization;
using BolsoEmDia.Front.Models.Request.Orcamento;
using BolsoEmDia.Front.Models.Response.Orcamento;

namespace BolsoEmDia.Front.Services.Servicos.Orcamento
{
    public class OrcamentoService : IOrcamentoService
    {
        private const string RotaBase = "api/v1/orcamentos";
        private readonly IApiHttpService _api;

        public OrcamentoService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<OrcamentoResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<OrcamentoResponse>>(RotaBase, ct);

        public Task<OrcamentoResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<OrcamentoResponse>($"{RotaBase}/{id}", ct);

        public Task<ProgressoOrcamentoResponse?> ObterProgresso(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default)
        {
            var mes = mesReferencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return _api.GetAsync<ProgressoOrcamentoResponse>(
                $"{RotaBase}/progresso?idCategoria={idCategoria}&mesReferencia={mes}", ct);
        }

        public async Task<OrcamentoResponse?> Definir(DefinirOrcamentoRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<OrcamentoResponse, DefinirOrcamentoRequest>(RotaBase, request, ct);
            return resposta;
        }

        public Task<bool> Atualizar(int id, AtualizarOrcamentoRequest request, CancellationToken ct = default) =>
            _api.PutAsync($"{RotaBase}/{id}", request, ct);

        public Task<bool> Ativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/ativar", new object(), ct);

        public Task<bool> Inativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/inativar", new object(), ct);
    }
}
