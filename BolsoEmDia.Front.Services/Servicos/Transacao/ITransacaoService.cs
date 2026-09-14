using BolsoEmDia.Front.Models.Enum;
using BolsoEmDia.Front.Models.Request.Transacao;
using BolsoEmDia.Front.Models.Response.Transacao;

namespace BolsoEmDia.Front.Services.Servicos.Transacao
{
    public interface ITransacaoService
    {
        Task<PaginatedResponse<TransacaoResponse>?> ObterPaginado(
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
            CancellationToken ct = default);

        Task<TransacaoResponse?> RegistrarReceita(CriarReceitaRequest request, CancellationToken ct = default);
        Task<TransacaoResponse?> RegistrarDespesa(CriarDespesaRequest request, CancellationToken ct = default);
    }
}
