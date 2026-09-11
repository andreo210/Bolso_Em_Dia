using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.RecorrenciaServices
{
    public interface IRecorrenciaService
    {
        Task<RecorrenciaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<RecorrenciaDto>> ObterTodosAsync(CancellationToken ct = default);

        // UC18 — Criar recorrência
        Task<RecorrenciaDto?> CriarAsync(CriarRecorrenciaDto dto, CancellationToken ct = default);

        // UC19 — Pausar / cancelar recorrência
        Task<bool> PausarAsync(int id, CancellationToken ct = default);

        // UC19 — Pausar / cancelar recorrência (reativar)
        Task<bool> ReativarAsync(int id, CancellationToken ct = default);
    }
}
