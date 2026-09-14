using BolsoEmDia.Front.Models.Request.Autenticacao;
using BolsoEmDia.Front.Models.Response.Autenticacao;
using BolsoEmDia.Front.Services.Servicos;

namespace BolsoEmDia.Front.Services.Servicos.Autenticacao
{
    public class AutenticacaoService : IAutenticacaoService
    {
        private const string RotaBase = "api/v1/auth";
        private readonly IApiHttpService _api;

        public AutenticacaoService(IApiHttpService api)
        {
            _api = api;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<LoginResponse, LoginRequest>($"{RotaBase}/login", request, ct);
            return resposta;
        }

        public Task<bool> RegistrarAsync(RegistrarRequest request, CancellationToken ct = default) =>
            _api.PostAsync($"{RotaBase}/registrar", request, ct);
    }
}
