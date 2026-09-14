namespace BolsoEmDia.Front.Models.Request.Transacao
{
    public class CriarDespesaRequest
    {
        public int IdConta { get; set; }
        public int IdCategoria { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
    }
}
