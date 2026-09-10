using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.OrcamentoServices
{
    public interface IOrcamentoService
    {
        Task<OrcamentoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<OrcamentoDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<OrcamentoDto?> DefinirAsync(DefinirOrcamentoDto dto, CancellationToken ct = default);

        Task<ProgressoOrcamentoDto?> ObterProgressoAsync(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default);

        // UC10 — Alertar orçamento estourado (estende UC04): retorna mensagem de alerta se estourou, null caso contrário ou sem orçamento definido.
        Task<string?> VerificarEstouroAsync(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default);
    }
}
