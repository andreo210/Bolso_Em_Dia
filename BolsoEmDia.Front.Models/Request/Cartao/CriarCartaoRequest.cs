namespace BolsoEmDia.Front.Models.Request.Cartao
{
    public class CriarCartaoRequest
    {
        public string Nome { get; set; } = string.Empty;
        public decimal LimiteTotal { get; set; }
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public int IdContaPagamento { get; set; }
    }
}
