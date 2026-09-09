using BolsoEmDia.Application.Models.Dto;

namespace BolsoEmDia.Application.Services.TransacaoServices
{
    public interface ITransacaoService
    {
        Task<TransacaoDto?> RegistrarReceitaAsync(CriarReceitaDto dto, CancellationToken ct = default);
    }
}
