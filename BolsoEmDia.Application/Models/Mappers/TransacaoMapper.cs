using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class TransacaoMapper
    {
        public static TransacaoDto ToDto(this Transacao entidade) => new()
        {
            IdTransacao = entidade.IdTransacao,
            IdConta = entidade.IdConta,
            IdCategoria = entidade.IdCategoria,
            Tipo = entidade.Tipo,
            Data = entidade.Data,
            Valor = entidade.Valor,
            Descricao = entidade.Descricao
        };

        public static List<TransacaoDto> ToDtoList(this IEnumerable<Transacao> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
