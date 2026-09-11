using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class CompraMapper
    {
        public static ParcelaDto ToDto(this Parcela entidade) => new()
        {
            IdParcela = entidade.IdParcela,
            IdFatura = entidade.IdFatura,
            Numero = entidade.Numero,
            Valor = entidade.Valor
        };

        public static CompraDto ToDto(this Compra entidade) => new()
        {
            IdCompra = entidade.IdCompra,
            IdCartao = entidade.IdCartao,
            IdCategoria = entidade.IdCategoria,
            Data = entidade.Data,
            Descricao = entidade.Descricao,
            ValorTotal = entidade.ValorTotal,
            NumeroParcelas = entidade.NumeroParcelas,
            Parcelas = entidade.Parcelas.OrderBy(p => p.Numero).Select(ToDto).ToList()
        };

        public static List<CompraDto> ToDtoList(this IEnumerable<Compra> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
