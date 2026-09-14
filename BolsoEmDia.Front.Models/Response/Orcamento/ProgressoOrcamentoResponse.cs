namespace BolsoEmDia.Front.Models.Response.Orcamento
{
    public class ProgressoOrcamentoResponse
    {
        public int IdCategoria { get; set; }
        public DateOnly MesReferencia { get; set; }
        public decimal ValorMeta { get; set; }
        public decimal TotalGasto { get; set; }
        public decimal PercentualConsumido { get; set; }
        public bool Estourado { get; set; }
    }
}
