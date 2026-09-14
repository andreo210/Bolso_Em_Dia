using BolsoEmDia.Front.Models.Request.Compra;
using BolsoEmDia.Front.Models.Response.Compra;

namespace BolsoEmDia.Front.Services.Servicos.Compra
{
    public class CompraService : ICompraService
    {
        private const string RotaBase = "api/v1/compras";
        private readonly IApiHttpService _api;

        public CompraService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<CompraResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<CompraResponse>>(RotaBase, ct);

        public Task<CompraResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<CompraResponse>($"{RotaBase}/{id}", ct);

        public async Task<CompraResponse?> Registrar(CriarCompraRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<CompraResponse, CriarCompraRequest>(RotaBase, request, ct);
            return resposta;
        }
    }
}
