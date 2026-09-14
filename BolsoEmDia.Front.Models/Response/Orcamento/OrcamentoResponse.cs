namespace BolsoEmDia.Front.Models.Response.Orcamento
{
    public class OrcamentoResponse
    {
        public int IdOrcamento { get; set; }
        public int IdCategoria { get; set; }
        public DateOnly MesReferencia { get; set; }
        public decimal ValorMeta { get; set; }
    }
}
