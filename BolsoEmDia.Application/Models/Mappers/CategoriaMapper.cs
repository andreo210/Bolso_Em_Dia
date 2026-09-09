using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class CategoriaMapper
    {
        public static CategoriaDto ToDto(this Categoria entidade) => new()
        {
            IdCategoria = entidade.IdCategoria,
            Nome = entidade.Nome,
            Tipo = entidade.Tipo,
            IdCategoriaPai = entidade.IdCategoriaPai,
            Ativa = entidade.Ativa
        };

        public static List<CategoriaDto> ToDtoList(this IEnumerable<Categoria> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
