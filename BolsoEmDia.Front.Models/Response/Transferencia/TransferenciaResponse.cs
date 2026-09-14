namespace BolsoEmDia.Front.Models.Response.Transferencia
{
    public class TransferenciaResponse
    {
        public int IdTransferencia { get; set; }
        public int IdContaOrigem { get; set; }
        public int IdContaDestino { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
    }
}
