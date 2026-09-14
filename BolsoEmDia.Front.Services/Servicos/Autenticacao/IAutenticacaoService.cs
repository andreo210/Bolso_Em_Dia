using BolsoEmDia.Front.Models.Request.Autenticacao;
using BolsoEmDia.Front.Models.Response.Autenticacao;

namespace BolsoEmDia.Front.Services.Servicos.Autenticacao
{
    public interface IAutenticacaoService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<bool> RegistrarAsync(RegistrarRequest request, CancellationToken ct = default);
    }
}
