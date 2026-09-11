using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class CartaoMapper
    {
        public static CartaoDto ToDto(this Cartao entidade) => new()
        {
            IdCartao = entidade.IdCartao,
            IdContaPagamento = entidade.IdContaPagamento,
            Nome = entidade.Nome,
            LimiteTotal = entidade.LimiteTotal,
            DiaFechamento = entidade.DiaFechamento,
            DiaVencimento = entidade.DiaVencimento,
            Ativo = entidade.Ativo
        };

        public static List<CartaoDto> ToDtoList(this IEnumerable<Cartao> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
