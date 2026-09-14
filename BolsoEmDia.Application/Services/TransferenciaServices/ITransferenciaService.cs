using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain;

namespace BolsoEmDia.Application.Services.TransferenciaServices
{
    public interface ITransferenciaService
    {
        Task<TransferenciaDto?> RegistrarAsync(CriarTransferenciaDto dto, CancellationToken ct = default);

        Task<PaginatedResult<TransferenciaDto>> ObterPaginadoAsync(
            ConsultaPaginadaRequest consulta,
            int? idConta = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            CancellationToken ct = default);
    }
}
