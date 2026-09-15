using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.CategoriaServices
{
    public interface ICategoriaService
    {
        Task<CategoriaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<CategoriaDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<CategoriaDto?> CriarAsync(CriarCategoriaDto dto, CancellationToken ct = default);

        Task<bool> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken ct = default);

        Task<bool> AtivarAsync(int id, CancellationToken ct = default);

        Task<bool> InativarAsync(int id, CancellationToken ct = default);
    }
}
