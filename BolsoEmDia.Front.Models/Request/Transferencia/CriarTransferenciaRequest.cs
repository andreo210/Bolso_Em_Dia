namespace BolsoEmDia.Front.Models.Request.Transferencia
{
    public class CriarTransferenciaRequest
    {
        public int IdContaOrigem { get; set; }
        public int IdContaDestino { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
    }
}
