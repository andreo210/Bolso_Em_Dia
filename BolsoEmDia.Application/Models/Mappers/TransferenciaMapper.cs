using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class TransferenciaMapper
    {
        public static TransferenciaDto ToDto(this Transferencia entidade) => new()
        {
            IdTransferencia = entidade.IdTransferencia,
            IdContaOrigem = entidade.IdContaOrigem,
            IdContaDestino = entidade.IdContaDestino,
            Data = entidade.Data,
            Valor = entidade.Valor,
            Descricao = entidade.Descricao
        };
    }
}
