using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.DashboardServices
{
    public interface IDashboardService
    {
        // UC22 — Ver dashboard
        Task<DashboardDto> ObterAsync(CancellationToken ct = default);
    }
}
