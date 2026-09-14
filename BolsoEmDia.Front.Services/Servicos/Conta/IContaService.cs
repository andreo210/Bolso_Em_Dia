using BolsoEmDia.Front.Models.Request.Conta;
using BolsoEmDia.Front.Models.Response.Conta;

namespace BolsoEmDia.Front.Services.Servicos.Conta
{
    public interface IContaService
    {
        Task<List<ContaResponse>?> ObterTodos(CancellationToken ct = default);
        Task<ContaResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<SaldoContaResponse?> ObterSaldo(int id, CancellationToken ct = default);
        Task<ContaResponse?> Inserir(CriarContaRequest request, CancellationToken ct = default);
        Task<bool> Atualizar(int id, AtualizarContaRequest request, CancellationToken ct = default);
        Task<bool> Ativar(int id, CancellationToken ct = default);
        Task<bool> Inativar(int id, CancellationToken ct = default);
    }
}
