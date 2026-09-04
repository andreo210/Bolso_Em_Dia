using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using Microsoft.Extensions.DependencyInjection;

namespace BolsoEmDia.Application.Extensions
{
    public static class DependencyInjectionApplicationExtensions
    {
        public static IServiceCollection AddInjecaoDependenciaApplicationsConfig(this IServiceCollection services)
        {
            services.AddScoped<INotificadorService, NotificadorService>();
            return services;
        }
    }
}
