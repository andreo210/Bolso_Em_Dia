using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class FaturaMapper
    {
        public static FaturaDto ToDto(this Fatura entidade) => new()
        {
            IdFatura = entidade.IdFatura,
            IdCartao = entidade.IdCartao,
            IdTransacaoPagamento = entidade.IdTransacaoPagamento,
            MesReferencia = entidade.MesReferencia,
            DataFechamento = entidade.DataFechamento,
            DataVencimento = entidade.DataVencimento,
            ValorTotal = entidade.ValorTotal,
            Status = entidade.Status
        };

        public static List<FaturaDto> ToDtoList(this IEnumerable<Fatura> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
