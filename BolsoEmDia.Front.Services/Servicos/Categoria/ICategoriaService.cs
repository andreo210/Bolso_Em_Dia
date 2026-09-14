using BolsoEmDia.Front.Models.Response.Categoria;

namespace BolsoEmDia.Front.Services.Servicos.Categoria
{
    // Mínimo necessário para alimentar o combo de categoria da tela de transações.
    // CRUD completo (UC07) fica para quando a tela de Categorias for construída.
    public interface ICategoriaService
    {
        Task<List<CategoriaResponse>?> ObterTodos(CancellationToken ct = default);
    }
}
