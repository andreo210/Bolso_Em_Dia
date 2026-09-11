using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.CartaoServices
{
    public interface ICartaoService
    {
        Task<CartaoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<CartaoDto>> ObterTodosAsync(CancellationToken ct = default);

        // UC13 — Cadastrar cartão de crédito
        Task<CartaoDto?> CriarAsync(CriarCartaoDto dto, CancellationToken ct = default);
    }
}
