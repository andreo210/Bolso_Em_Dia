using System.ComponentModel.DataAnnotations;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Dto
{
    public class TransacaoDto
    {
        public int IdTransacao { get; set; }
        public int IdConta { get; set; }
        public int? IdCategoria { get; set; }
        public TipoTransacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }

        // UC10 — preenchido só quando a despesa estoura o orçamento da categoria no mês; nunca bloqueia (ver UC04).
        public string? AlertaOrcamento { get; set; }
    }

    public class CriarReceitaDto
    {
        [Required(ErrorMessage = "Conta é obrigatória")]
        public int IdConta { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [MaxLength(255)]
        public string? Descricao { get; set; }
    }

    public class CriarDespesaDto
    {
        [Required(ErrorMessage = "Conta é obrigatória")]
        public int IdConta { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [MaxLength(255)]
        public string? Descricao { get; set; }
    }
}
