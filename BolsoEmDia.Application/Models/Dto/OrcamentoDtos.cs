using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class OrcamentoDto
    {
        public int IdOrcamento { get; set; }
        public int IdCategoria { get; set; }
        public DateOnly MesReferencia { get; set; }
        public decimal ValorMeta { get; set; }
    }

    public class DefinirOrcamentoDto
    {
        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Mês de referência é obrigatório")]
        public DateOnly MesReferencia { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor da meta deve ser maior que zero")]
        public decimal ValorMeta { get; set; }
    }

    public class ProgressoOrcamentoDto
    {
        public int IdCategoria { get; set; }
        public DateOnly MesReferencia { get; set; }
        public decimal ValorMeta { get; set; }
        public decimal TotalGasto { get; set; }
        public decimal PercentualConsumido { get; set; }
        public bool Estourado { get; set; }
    }
}
