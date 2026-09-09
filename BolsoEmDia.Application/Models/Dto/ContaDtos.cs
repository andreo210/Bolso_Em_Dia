using System.ComponentModel.DataAnnotations;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Dto
{
    public class ContaDto
    {
        public int IdConta { get; set; }
        public string Nome { get; set; } = null!;
        public TipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public bool Ativa { get; set; }
    }

    public class CriarContaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Tipo é obrigatório")]
        public TipoConta Tipo { get; set; }

        public decimal SaldoInicial { get; set; }
    }

    public class AtualizarContaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;
    }

    public class SaldoContaDto
    {
        public int IdConta { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataReferencia { get; set; }
    }
}
