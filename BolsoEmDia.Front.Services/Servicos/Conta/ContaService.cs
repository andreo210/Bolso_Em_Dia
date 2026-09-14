using BolsoEmDia.Front.Models.Request.Conta;
using BolsoEmDia.Front.Models.Response.Conta;

namespace BolsoEmDia.Front.Services.Servicos.Conta
{
    public class ContaService : IContaService
    {
        private const string RotaBase = "api/v1/contas";
        private readonly IApiHttpService _api;

        public ContaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<List<ContaResponse>?> ObterTodos(CancellationToken ct = default) =>
            _api.GetAsync<List<ContaResponse>>(RotaBase, ct);

        public Task<ContaResponse?> ObterPorId(int id, CancellationToken ct = default) =>
            _api.GetAsync<ContaResponse>($"{RotaBase}/{id}", ct);

        public Task<SaldoContaResponse?> ObterSaldo(int id, CancellationToken ct = default) =>
            _api.GetAsync<SaldoContaResponse>($"{RotaBase}/{id}/saldo", ct);

        public async Task<ContaResponse?> Inserir(CriarContaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<ContaResponse, CriarContaRequest>(RotaBase, request, ct);
            return resposta;
        }

        public Task<bool> Atualizar(int id, AtualizarContaRequest request, CancellationToken ct = default) =>
            _api.PutAsync($"{RotaBase}/{id}", request, ct);

        public Task<bool> Ativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/ativar", new object(), ct);

        public Task<bool> Inativar(int id, CancellationToken ct = default) =>
            _api.PatchAsync($"{RotaBase}/{id}/inativar", new object(), ct);
    }
}
