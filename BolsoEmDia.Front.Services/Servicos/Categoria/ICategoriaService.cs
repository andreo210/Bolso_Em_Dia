using BolsoEmDia.Front.Models.Request.Categoria;
using BolsoEmDia.Front.Models.Response.Categoria;

namespace BolsoEmDia.Front.Services.Servicos.Categoria
{
    public interface ICategoriaService
    {
        Task<List<CategoriaResponse>?> ObterTodos(CancellationToken ct = default);
        Task<CategoriaResponse?> ObterPorId(int id, CancellationToken ct = default);
        Task<CategoriaResponse?> Inserir(CriarCategoriaRequest request, CancellationToken ct = default);
        Task<bool> Atualizar(int id, AtualizarCategoriaRequest request, CancellationToken ct = default);
        Task<bool> Ativar(int id, CancellationToken ct = default);
        Task<bool> Inativar(int id, CancellationToken ct = default);
    }
}
