using BolsoEmDia.Front.Models.Response.Dashboard;

namespace BolsoEmDia.Front.Services.Servicos.Dashboard
{
    public interface IDashboardService
    {
        // UC22 — Ver dashboard
        Task<DashboardResponse?> Obter(CancellationToken ct = default);
    }
}
