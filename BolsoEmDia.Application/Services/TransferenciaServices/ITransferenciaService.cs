using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.TransferenciaServices
{
    public interface ITransferenciaService
    {
        Task<TransferenciaDto?> RegistrarAsync(CriarTransferenciaDto dto, CancellationToken ct = default);
    }
}
