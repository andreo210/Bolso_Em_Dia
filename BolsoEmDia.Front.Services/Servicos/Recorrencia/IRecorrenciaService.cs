using BolsoEmDia.Front.Models.Request.Recorrencia;
using BolsoEmDia.Front.Models.Response.Recorrencia;

namespace BolsoEmDia.Front.Services.Servicos.Recorrencia
{
    public interface IRecorrenciaService
    {
        Task<List<RecorrenciaResponse>?> ObterTodos(CancellationToken ct = default);
        Task<RecorrenciaResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<RecorrenciaResponse?> Inserir(CriarRecorrenciaRequest request, CancellationToken ct = default);
        Task<bool> Pausar(int id, CancellationToken ct = default);
        Task<bool> Reativar(int id, CancellationToken ct = default);
    }
}
