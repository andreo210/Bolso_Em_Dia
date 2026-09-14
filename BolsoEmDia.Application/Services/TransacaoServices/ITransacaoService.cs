using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Services.TransacaoServices
{
    public interface ITransacaoService
    {
        Task<TransacaoDto?> RegistrarReceitaAsync(CriarReceitaDto dto, CancellationToken ct = default);

        Task<TransacaoDto?> RegistrarDespesaAsync(CriarDespesaDto dto, CancellationToken ct = default);

        Task<PaginatedResult<TransacaoDto>> ObterPaginadoAsync(
            ConsultaPaginadaRequest consulta,
            int? idConta = null,
            int? idCategoria = null,
            TipoTransacao? tipo = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            CancellationToken ct = default);
    }
}
