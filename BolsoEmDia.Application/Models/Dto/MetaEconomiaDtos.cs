using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class MetaEconomiaDto
    {
        public int IdMeta { get; set; }
        public int? IdConta { get; set; }
        public string Nome { get; set; } = null!;
        public decimal ValorAlvo { get; set; }
        public DateTime? DataAlvo { get; set; }
        public bool Concluida { get; set; }
        public decimal TotalAportado { get; set; }
    }

    public class CriarMetaEconomiaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor alvo deve ser maior que zero")]
        public decimal ValorAlvo { get; set; }

        public DateTime? DataAlvo { get; set; }

        public int? IdConta { get; set; }
    }

    public class AporteMetaDto
    {
        public int IdAporte { get; set; }
        public int IdMeta { get; set; }
        public int? IdTransacao { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
    }

    public class RegistrarAporteMetaDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Data { get; set; }

        public int? IdTransacao { get; set; }
    }
}
