namespace BolsoEmDia.Front.Models.Response.Cartao
{
    public class CartaoResponse
    {
        public int IdCartao { get; set; }
        public int IdContaPagamento { get; set; }
        public string Nome { get; set; } = null!;
        public decimal LimiteTotal { get; set; }
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public bool Ativo { get; set; }
    }
}
