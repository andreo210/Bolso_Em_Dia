using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.CategoriaServices
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public CategoriaService(
            ICategoriaRepository categoriaRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _categoriaRepository = categoriaRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<CategoriaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var categoria = await ObterCategoriaDoUsuarioAsync(id, ct);
            return categoria?.ToDto();
        }

        public async Task<IReadOnlyList<CategoriaDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var categorias = await _categoriaRepository.ObterAsync(c => c.IdUsuario == IdUsuarioAtual, ct: ct);
            return categorias.ToDtoList();
        }

        // UC07 — Cadastrar categoria
        public async Task<CategoriaDto?> CriarAsync(CriarCategoriaDto dto, CancellationToken ct = default)
        {
            if (dto.IdCategoriaPai.HasValue && !await ValidarCategoriaPaiAsync(dto.IdCategoriaPai.Value, dto.Tipo, ct))
                return null;

            var categoria = Categoria.Criar(IdUsuarioAtual, dto.Nome, dto.Tipo, dto.IdCategoriaPai);
            var salva = await _categoriaRepository.InserirSalvarAsync(categoria, ct);
            return salva.ToDto();
        }

        // E1 — categoria-pai deve existir, pertencer ao usuário, ter o mesmo Tipo e não ser
        // ela própria uma subcategoria (hierarquia de um nível só).
        private async Task<bool> ValidarCategoriaPaiAsync(int idCategoriaPai, TipoCategoria tipo, CancellationToken ct)
        {
            var categoriaPai = await ObterCategoriaDoUsuarioAsync(idCategoriaPai, ct);
            if (categoriaPai is null)
            {
                _notificador.Add("Categoria pai não encontrada");
                return false;
            }

            if (categoriaPai.Tipo != tipo)
            {
                _notificador.Add("Categoria pai deve ser do mesmo tipo");
                return false;
            }

            if (categoriaPai.IdCategoriaPai.HasValue)
            {
                _notificador.Add("Categoria pai não pode ser, ela própria, uma subcategoria");
                return false;
            }

            return true;
        }

        private Task<Categoria?> ObterCategoriaDoUsuarioAsync(int id, CancellationToken ct)
            => _categoriaRepository.ObterPrimeiroAsync(c => c.IdCategoria == id && c.IdUsuario == IdUsuarioAtual, ct: ct);
    }
}
