using BolsoEmDia.Front.Services.Configuration;
using BolsoEmDia.Front.Services.Servicos;
using BolsoEmDia.Front.Services.Servicos.Autenticacao;
using BolsoEmDia.Front.Services.Servicos.Categoria;
using BolsoEmDia.Front.Services.Servicos.Conta;
using BolsoEmDia.Front.Services.Servicos.Meta;
using BolsoEmDia.Front.Services.Servicos.Orcamento;
using BolsoEmDia.Front.Services.Servicos.Transacao;
using BolsoEmDia.Front.Services.Servicos.Transferencia;
using BolsoEmDia.Front.Services.Utils.Notificacao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BolsoEmDia.Front.Services.Extensions
{
    public static class DependencyInjectionServiceExtensions
    {
        /// <summary>
        /// Registro único do front. Tudo Scoped: em Blazor Server o escopo é o
        /// circuito, então serviço com estado (notificação, diálogo) vive enquanto
        /// a aba estiver aberta — que é exatamente o desejado.
        /// </summary>
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IConfirmDialogService, ConfirmDialogService>();

            services.Configure<ApiConfig>(configuration.GetSection("ApiConfig"));
            services.AddHttpContextAccessor();
            services.AddScoped<JwtAuthorizationHandler>();

            var apiConfig = configuration.GetSection("ApiConfig").Get<ApiConfig>();

            // Falhar aqui, na subida, é melhor do que descobrir pela primeira tela
            // que devolve erro de conexão sem dizer por quê.
            if (string.IsNullOrWhiteSpace(apiConfig?.BaseUrlApiLocacao))
                throw new InvalidOperationException("ApiConfig:BaseUrlApiLocacao não configurado.");

            // Cliente tipado: o HttpClientFactory cuida do pool de conexões.
            // `new HttpClient()` avulso prende socket em TIME_WAIT e ignora mudança de DNS.
            services.AddHttpClient<IApiHttpService, ApiHttpService>(client =>
            {
                client.BaseAddress = new Uri(apiConfig.BaseUrlApiLocacao);
            })
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

            services.AddScoped<IAutenticacaoService, AutenticacaoService>();

            // Um registro por área — a partir daqui é o CRUD do projeto:
            services.AddScoped<IContaService, ContaService>();
            services.AddScoped<ITransacaoService, TransacaoService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ITransferenciaService, TransferenciaService>();
            services.AddScoped<IOrcamentoService, OrcamentoService>();
            services.AddScoped<IMetaEconomiaService, MetaEconomiaService>();

            return services;
        }
    }
}
