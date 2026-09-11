using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Services.AutenticacaoServices;
using BolsoEmDia.Application.Services.CartaoServices;
using BolsoEmDia.Application.Services.CategoriaServices;
using BolsoEmDia.Application.Services.CompraServices;
using BolsoEmDia.Application.Services.ContaServices;
using BolsoEmDia.Application.Services.FaturaServices;
using BolsoEmDia.Application.Services.MetaEconomiaServices;
using BolsoEmDia.Application.Services.OrcamentoServices;
using BolsoEmDia.Application.Services.RecorrenciaServices;
using BolsoEmDia.Application.Services.TransacaoServices;
using BolsoEmDia.Application.Services.TransferenciaServices;
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
            services.AddScoped<ITransferenciaService, TransferenciaService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IOrcamentoService, OrcamentoService>();
            services.AddScoped<IMetaEconomiaService, MetaEconomiaService>();
            services.AddScoped<ICartaoService, CartaoService>();
            services.AddScoped<ICompraService, CompraService>();
            services.AddScoped<IFaturaService, FaturaService>();
            services.AddScoped<IFechamentoFaturaJobService, FechamentoFaturaJobService>();
            services.AddScoped<IRecorrenciaService, RecorrenciaService>();
            services.AddScoped<IRecorrenciaJobService, RecorrenciaJobService>();
            return services;
        }
    }
}
