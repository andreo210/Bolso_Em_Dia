using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.ContaServices
{
    public interface IContaService
    {
        Task<ContaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<ContaDto>> ObterTodosAsync(CancellationToken ct = default);

        Task<SaldoContaDto?> ObterSaldoAsync(int id, CancellationToken ct = default);

        Task<ContaDto?> CriarAsync(CriarContaDto dto, CancellationToken ct = default);

        Task<bool> AtualizarAsync(int id, AtualizarContaDto dto, CancellationToken ct = default);

        Task<bool> AtivarAsync(int id, CancellationToken ct = default);

        Task<bool> InativarAsync(int id, CancellationToken ct = default);
    }
}
