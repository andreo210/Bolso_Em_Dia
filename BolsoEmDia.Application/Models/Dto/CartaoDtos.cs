using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class CartaoDto
    {
        public int IdCartao { get; set; }
        public int IdContaPagamento { get; set; }
        public string Nome { get; set; } = null!;
        public decimal LimiteTotal { get; set; }
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public bool Ativo { get; set; }
    }

    public class CriarCartaoDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Limite total deve ser maior que zero")]
        public decimal LimiteTotal { get; set; }

        [Range(1, 31, ErrorMessage = "Dia de fechamento deve estar entre 1 e 31")]
        public int DiaFechamento { get; set; }

        [Range(1, 31, ErrorMessage = "Dia de vencimento deve estar entre 1 e 31")]
        public int DiaVencimento { get; set; }

        public int IdContaPagamento { get; set; }
    }
}
