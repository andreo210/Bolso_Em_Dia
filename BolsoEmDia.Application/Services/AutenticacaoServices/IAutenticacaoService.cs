using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.AutenticacaoServices
{
    public interface IAutenticacaoService
    {
        Task<TokenDto?> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken ct = default);

        Task<TokenDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);
    }
}
