using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class MetaEconomiaMapper
    {
        public static MetaEconomiaDto ToDto(this MetaEconomia entidade) => new()
        {
            IdMeta = entidade.IdMeta,
            IdConta = entidade.IdConta,
            Nome = entidade.Nome,
            ValorAlvo = entidade.ValorAlvo,
            DataAlvo = entidade.DataAlvo,
            Concluida = entidade.Concluida,
            TotalAportado = entidade.TotalAportado()
        };

        public static List<MetaEconomiaDto> ToDtoList(this IEnumerable<MetaEconomia> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();

        public static AporteMetaDto ToDto(this AporteMeta entidade) => new()
        {
            IdAporte = entidade.IdAporte,
            IdMeta = entidade.IdMeta,
            IdTransacao = entidade.IdTransacao,
            Data = entidade.Data,
            Valor = entidade.Valor
        };

        public static List<AporteMetaDto> ToDtoList(this IEnumerable<AporteMeta> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
