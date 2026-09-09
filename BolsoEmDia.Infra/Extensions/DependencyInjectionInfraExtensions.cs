using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data;
using BolsoEmDia.Infra.Data.CurrentUsers;
using BolsoEmDia.Infra.Data.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BolsoEmDia.Infra.Extensions
{
    public static class DependencyInjectionInfraExtensions
    {
        public static IServiceCollection AddPostgresDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString,
                    npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null));
                options.UseSnakeCaseNamingConvention();
            });

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IContaRepository, ContaRepository>();
            services.AddScoped<ITransacaoRepository, TransacaoRepository>();
            services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
            services.AddScoped<IMetaEconomiaRepository, MetaEconomiaRepository>();
            services.AddScoped<IAporteMetaRepository, AporteMetaRepository>();
            services.AddScoped<ICartaoRepository, CartaoRepository>();
            services.AddScoped<IFaturaRepository, FaturaRepository>();
            services.AddScoped<ICompraRepository, CompraRepository>();
            services.AddScoped<IParcelaRepository, ParcelaRepository>();
            services.AddScoped<IRecorrenciaRepository, RecorrenciaRepository>();

            return services;
        }
    }
}
