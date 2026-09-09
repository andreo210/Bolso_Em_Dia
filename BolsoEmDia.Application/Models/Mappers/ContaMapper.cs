using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Mappers
{
    public static class ContaMapper
    {
        public static ContaDto ToDto(this Conta entidade) => new()
        {
            IdConta = entidade.IdConta,
            Nome = entidade.Nome,
            Tipo = entidade.Tipo,
            SaldoInicial = entidade.SaldoInicial,
            Ativa = entidade.Ativa
        };

        public static List<ContaDto> ToDtoList(this IEnumerable<Conta> entidades)
            => entidades is null ? new() : entidades.Select(ToDto).ToList();
    }
}
