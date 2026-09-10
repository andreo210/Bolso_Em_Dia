using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Services.AutenticacaoServices;
using BolsoEmDia.Application.Services.CategoriaServices;
using BolsoEmDia.Application.Services.ContaServices;
using BolsoEmDia.Application.Services.OrcamentoServices;
using BolsoEmDia.Application.Services.TransacaoServices;
using Microsoft.Extensions.DependencyInjection;

namespace BolsoEmDia.Application.Extensions
{
    public static class DependencyInjectionApplicationExtensions
    {
        public static IServiceCollection AddInjecaoDependenciaApplicationsConfig(this IServiceCollection services)
        {
            services.AddScoped<INotificadorService, NotificadorService>();
            services.AddScoped<IContaService, ContaService>();
            services.AddScoped<IAutenticacaoService, AutenticacaoService>();
            services.AddScoped<ITransacaoService, TransacaoService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IOrcamentoService, OrcamentoService>();
            return services;
        }
    }
}
