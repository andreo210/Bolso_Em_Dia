using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Response.Recorrencia
{
    public class RecorrenciaResponse
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
}
