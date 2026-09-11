using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class ParcelaDto
    {
        public int IdParcela { get; set; }
        public int IdFatura { get; set; }
        public int Numero { get; set; }
        public decimal Valor { get; set; }
    }

    public class CompraDto
    {
        public int IdCompra { get; set; }
        public int IdCartao { get; set; }
        public int IdCategoria { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = null!;
        public decimal ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public List<ParcelaDto> Parcelas { get; set; } = new();
    }

    public class CriarCompraDto
    {
        public int IdCartao { get; set; }

        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "Descrição é obrigatória")]
        public string Descricao { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor total deve ser maior que zero")]
        public decimal ValorTotal { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Número de parcelas deve ser maior ou igual a 1")]
        public int NumeroParcelas { get; set; } = 1;
    }
}
