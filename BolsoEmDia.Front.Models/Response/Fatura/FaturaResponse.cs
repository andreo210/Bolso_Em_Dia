using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Response.Fatura
{
    public class FaturaResponse
    {
        public int IdFatura { get; set; }
        public int IdCartao { get; set; }
        public int? IdTransacaoPagamento { get; set; }
        public DateOnly MesReferencia { get; set; }
        public DateTime DataFechamento { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusFatura Status { get; set; }
    }
}
