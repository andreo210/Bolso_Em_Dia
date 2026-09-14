using BolsoEmDia.Front.Models.Request.Compra;
using BolsoEmDia.Front.Models.Response.Compra;

namespace BolsoEmDia.Front.Services.Servicos.Compra
{
    public interface ICompraService
    {
        Task<List<CompraResponse>?> ObterTodos(CancellationToken ct = default);
        Task<CompraResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<CompraResponse?> Registrar(CriarCompraRequest request, CancellationToken ct = default);
    }
}
