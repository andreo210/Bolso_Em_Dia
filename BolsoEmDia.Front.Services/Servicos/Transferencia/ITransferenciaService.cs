using BolsoEmDia.Front.Models.Request.Transferencia;
using BolsoEmDia.Front.Models.Response.Transferencia;

namespace BolsoEmDia.Front.Services.Servicos.Transferencia
{
    public interface ITransferenciaService
    {
        Task<PaginatedResponse<TransferenciaResponse>?> ObterPaginado(
            int pagina = 1,
            int itensPorPagina = 10,
            string? termo = null,
            string? ordenarPor = null,
            string? direcao = null,
            int? idConta = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            CancellationToken ct = default);

        Task<TransferenciaResponse?> Registrar(CriarTransferenciaRequest request, CancellationToken ct = default);
    }
}
