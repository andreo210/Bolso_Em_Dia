using BolsoEmDia.Front.Models.Request.Cartao;
using BolsoEmDia.Front.Models.Response.Cartao;

namespace BolsoEmDia.Front.Services.Servicos.Cartao
{
    public interface ICartaoService
    {
        Task<List<CartaoResponse>?> ObterTodos(CancellationToken ct = default);
        Task<CartaoResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<CartaoResponse?> Inserir(CriarCartaoRequest request, CancellationToken ct = default);
    }
}
