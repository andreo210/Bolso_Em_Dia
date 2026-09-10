using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class TransferenciaDto
    {
        public int IdTransferencia { get; set; }
        public int IdContaOrigem { get; set; }
        public int IdContaDestino { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
    }

    public class CriarTransferenciaDto
    {
        [Required(ErrorMessage = "Conta de origem é obrigatória")]
        public int IdContaOrigem { get; set; }

        [Required(ErrorMessage = "Conta de destino é obrigatória")]
        public int IdContaDestino { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [MaxLength(255)]
        public string? Descricao { get; set; }
    }
}
