using BolsoEmDia.Front.Models.Request.Fatura;
using BolsoEmDia.Front.Models.Response.Fatura;

namespace BolsoEmDia.Front.Services.Servicos.Fatura
{
    public interface IFaturaService
    {
        Task<List<FaturaResponse>?> ObterTodos(CancellationToken ct = default);
        Task<FaturaResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<FaturaResponse?> RegistrarPagamento(int id, PagarFaturaRequest request, CancellationToken ct = default);
    }
}
