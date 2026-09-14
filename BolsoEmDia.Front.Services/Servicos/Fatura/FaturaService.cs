using BolsoEmDia.Front.Models.Request.Fatura;
using BolsoEmDia.Front.Models.Response.Fatura;

namespace BolsoEmDia.Front.Services.Servicos.Fatura
{
    public class FaturaService : IFaturaService
    {
        private const string RotaBase = "api/v1/faturas";
        private readonly IApiHttpService _api;

        public FaturaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<FaturaResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<FaturaResponse>>(RotaBase, ct);

        public Task<FaturaResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<FaturaResponse>($"{RotaBase}/{id}", ct);

        public async Task<FaturaResponse?> RegistrarPagamento(int id, PagarFaturaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<FaturaResponse, PagarFaturaRequest>($"{RotaBase}/{id}/pagamento", request, ct);
            return resposta;
        }
    }
}
