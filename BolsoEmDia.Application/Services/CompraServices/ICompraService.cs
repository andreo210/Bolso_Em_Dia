using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.CompraServices
{
    public interface ICompraService
    {
        Task<CompraDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<CompraDto>> ObterTodosAsync(CancellationToken ct = default);

        // UC14 — Registrar compra no cartão (inclui UC15, UC16)
        Task<CompraDto?> RegistrarAsync(CriarCompraDto dto, CancellationToken ct = default);
    }
}
