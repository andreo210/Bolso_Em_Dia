using BolsoEmDia.Front.Models.Request.Transferencia;
using BolsoEmDia.Front.Models.Response.Transferencia;

namespace BolsoEmDia.Front.Services.Servicos.Transferencia
{
    public class TransferenciaService : ITransferenciaService
    {
        private const string RotaBase = "api/v1/transferencias";
        private readonly IApiHttpService _api;

        public TransferenciaService(IApiHttpService api)
        {
            _api = api;
        }

        public Task<PaginatedResponse<TransferenciaResponse>?> ObterPaginado(
            int pagina = 1,
            int itensPorPagina = 10,
            string? termo = null,
            string? ordenarPor = null,
            string? direcao = null,
            int? idConta = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            CancellationToken ct = default)
        {
            var query = new QueryPaginada(pagina, itensPorPagina, termo, ordenarPor, direcao)
                .Com("idConta", idConta)
                .Com("dataInicio", dataInicio)
                .Com("dataFim", dataFim);

            return _api.GetAsync<PaginatedResponse<TransferenciaResponse>>($"{RotaBase}{query}", ct);
        }

        public async Task<TransferenciaResponse?> Registrar(CriarTransferenciaRequest request, CancellationToken ct = default)
        {
            var (resposta, _) = await _api.PostAsync<TransferenciaResponse, CriarTransferenciaRequest>(RotaBase, request, ct);
            return resposta;
        }
    }
}
