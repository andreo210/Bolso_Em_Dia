using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.OrcamentoServices
{
    public interface IOrcamentoService
    {
        Task<OrcamentoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<OrcamentoDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<OrcamentoDto?> DefinirAsync(DefinirOrcamentoDto dto, CancellationToken ct = default);

        Task<ProgressoOrcamentoDto?> ObterProgressoAsync(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default);
    }
}
