namespace BolsoEmDia.Front.Models.Request.Compra
{
    public class CriarCompraRequest
    {
        public int IdCartao { get; set; }
        public int IdCategoria { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public int NumeroParcelas { get; set; } = 1;
    }
}
