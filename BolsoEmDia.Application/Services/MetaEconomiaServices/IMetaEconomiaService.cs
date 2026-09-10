using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.MetaEconomiaServices
{
    public interface IMetaEconomiaService
    {
        Task<MetaEconomiaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<MetaEconomiaDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<IReadOnlyList<AporteMetaDto>> ObterAportesAsync(int idMeta, CancellationToken ct = default);

        // UC11 — Criar meta de economia
        Task<MetaEconomiaDto?> CriarAsync(CriarMetaEconomiaDto dto, CancellationToken ct = default);

        // UC12 — Registrar aporte em meta
        Task<AporteMetaDto?> RegistrarAporteAsync(int idMeta, RegistrarAporteMetaDto dto, CancellationToken ct = default);
    }
}
