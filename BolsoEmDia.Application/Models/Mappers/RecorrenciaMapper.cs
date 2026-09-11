using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class RecorrenciaMapper
    {
        public static RecorrenciaDto ToDto(this Recorrencia entidade) => new()
        {
            IdRecorrencia = entidade.IdRecorrencia,
            IdConta = entidade.IdConta,
            IdCartao = entidade.IdCartao,
            IdCategoria = entidade.IdCategoria,
            TipoTransacao = entidade.TipoTransacao,
            Valor = entidade.Valor,
            Frequencia = entidade.Frequencia,
            DiaGeracao = entidade.DiaGeracao,
            DataInicio = entidade.DataInicio,
            DataFim = entidade.DataFim,
            Ativa = entidade.Ativa
        };

        public static List<RecorrenciaDto> ToDtoList(this IEnumerable<Recorrencia> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
