using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.FaturaServices
{
    public interface IFaturaService
    {
        Task<FaturaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<FaturaDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<FaturaDto?> RegistrarPagamentoAsync(int idFatura, PagarFaturaDto dto, CancellationToken ct = default);
    }
}
