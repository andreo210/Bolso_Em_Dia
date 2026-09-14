using System.ComponentModel.DataAnnotations;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Dto
{
    public class RecorrenciaDto
    {
        public int IdRecorrencia { get; set; }
        public int? IdConta { get; set; }
        public int? IdCartao { get; set; }
        public int IdCategoria { get; set; }
        public TipoTransacao? TipoTransacao { get; set; }
        public decimal Valor { get; set; }
        public FrequenciaRecorrencia Frequencia { get; set; }
        public int DiaGeracao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativa { get; set; }
    }

    public class CriarRecorrenciaDto
    {
        public int? IdConta { get; set; }

        public int? IdCartao { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int IdCategoria { get; set; }

        public TipoTransacao? TipoTransacao { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Frequência é obrigatória")]
        public FrequenciaRecorrencia Frequencia { get; set; }

        public int DiaGeracao { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }
    }
}
