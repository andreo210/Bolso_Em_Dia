using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class OrcamentoMapper
    {
        public static OrcamentoDto ToDto(this Orcamento entidade) => new()
        {
            IdOrcamento = entidade.IdOrcamento,
            IdCategoria = entidade.IdCategoria,
            MesReferencia = entidade.MesReferencia,
            ValorMeta = entidade.ValorMeta
        };

        public static List<OrcamentoDto> ToDtoList(this IEnumerable<Orcamento> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
