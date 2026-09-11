using System.ComponentModel.DataAnnotations;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Dto
{
    public class FaturaDto
    {
        public int IdFatura { get; set; }
        public int IdCartao { get; set; }
        public int? IdTransacaoPagamento { get; set; }
        public DateOnly MesReferencia { get; set; }
        public DateTime DataFechamento { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusFatura Status { get; set; }
    }

    public class PagarFaturaDto
    {
        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int IdCategoria { get; set; }
    }
}
