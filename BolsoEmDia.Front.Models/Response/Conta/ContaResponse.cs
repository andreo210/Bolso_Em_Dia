using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Response.Conta
{
    public class ContaResponse
    {
        public int IdConta { get; set; }
        public string Nome { get; set; } = null!;
        public TipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public bool Ativa { get; set; }
    }
}
