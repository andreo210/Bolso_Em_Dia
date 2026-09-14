using BolsoEmDia.Front.Models.Enum;
using BolsoEmDia.Front.Models.Request.Transacao;
using BolsoEmDia.Front.Models.Response.Transacao;

namespace BolsoEmDia.Front.Services.Servicos.Transacao
{
    public class TransacaoService : ITransacaoService
    {
        private const string RotaBase = "api/v1/transacoes";
        private readonly IApiHttpService _api;

        public TransacaoService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<PaginatedResponse<TransacaoResponse>?> ObterPaginado(
            int pagina = 1,
            int itensPorPagina = 10,
            string? termo = null,
            string? ordenarPor = null,
            string? direcao = null,
            int? idConta = null,
            int? idCategoria = null,
            TipoTransacao? tipo = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            CancellationToken ct = default)
        {
            var query = new QueryPaginada(pagina, itensPorPagina, termo, ordenarPor, direcao)
                .Com("idConta", idConta)
                .Com("idCategoria", idCategoria)
                .Com("tipo", tipo.HasValue ? (int)tipo.Value : (int?)null)
                .Com("dataInicio", dataInicio)
                .Com("dataFim", dataFim);

            return _api.GetAsync<PaginatedResponse<TransacaoResponse>>($"{RotaBase}{query}", ct);
        }

        public async Task<TransacaoResponse?> RegistrarReceita(CriarReceitaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<TransacaoResponse, CriarReceitaRequest>($"{RotaBase}/receitas", request, ct);
            return resposta;
        }

        public async Task<TransacaoResponse?> RegistrarDespesa(CriarDespesaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<TransacaoResponse, CriarDespesaRequest>($"{RotaBase}/despesas", request, ct);
            return resposta;
        }
    }
}
