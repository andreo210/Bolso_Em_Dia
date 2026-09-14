namespace BolsoEmDia.Front.Models.Request.Orcamento
{
    public class DefinirOrcamentoRequest
    {
        public int IdCategoria { get; set; }
        public DateOnly MesReferencia { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public decimal ValorMeta { get; set; }
    }
}
