using BolsoEmDia.Front.Models.Request.Categoria;
using BolsoEmDia.Front.Models.Response.Categoria;

namespace BolsoEmDia.Front.Services.Servicos.Categoria
{
    public class CategoriaService : ICategoriaService
    {
        private const string RotaBase = "api/v1/categorias";
        private readonly IApiHttpService _api;

        public CategoriaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<CategoriaResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<CategoriaResponse>>(RotaBase, ct);

        public Task<CategoriaResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<CategoriaResponse>($"{RotaBase}/{id}", ct);

        public async Task<CategoriaResponse?> Inserir(CriarCategoriaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<CategoriaResponse, CriarCategoriaRequest>(RotaBase, request, ct);
            return resposta;
        }

        public Task<bool> Atualizar(int id, AtualizarCategoriaRequest request, CancellationToken ct = default) =>
            _api.PutAsync($"{RotaBase}/{id}", request, ct);

        public Task<bool> Ativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/ativar", new object(), ct);

        public Task<bool> Inativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/inativar", new object(), ct);
    }
}
