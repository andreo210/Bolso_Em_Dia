using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Request.Recorrencia
{
    public class CriarRecorrenciaRequest
    {
        public int? IdConta { get; set; }
        public int? IdCartao { get; set; }
        public int IdCategoria { get; set; }
        public TipoTransacao? TipoTransacao { get; set; }
        public decimal Valor { get; set; }
        public FrequenciaRecorrencia Frequencia { get; set; }
        public int DiaGeracao { get; set; }
        public DateTime DataInicio { get; set; } = DateTime.Today;
        public DateTime? DataFim { get; set; }
    }
}
