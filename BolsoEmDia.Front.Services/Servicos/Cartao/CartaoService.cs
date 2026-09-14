using BolsoEmDia.Front.Models.Request.Cartao;
using BolsoEmDia.Front.Models.Response.Cartao;

namespace BolsoEmDia.Front.Services.Servicos.Cartao
{
    public class CartaoService : ICartaoService
    {
        private const string RotaBase = "api/v1/cartoes";
        private readonly IApiHttpService _api;

        public CartaoService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<CartaoResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<CartaoResponse>>(RotaBase, ct);

        public Task<CartaoResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<CartaoResponse>($"{RotaBase}/{id}", ct);

        public async Task<CartaoResponse?> Inserir(CriarCartaoRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<CartaoResponse, CriarCartaoRequest>(RotaBase, request, ct);
            return resposta;
        }
    }
}
