using BolsoEmDia.Front.Models.Response.Dashboard;

namespace BolsoEmDia.Front.Services.Servicos.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private const string RotaBase = "api/v1/dashboard";
        private readonly IApiHttpService _api;

        public DashboardService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<DashboardResponse?> Obter(CancellationToken ct = default) =>
            _api.GetAsync<DashboardResponse>(RotaBase, ct);
    }
}
