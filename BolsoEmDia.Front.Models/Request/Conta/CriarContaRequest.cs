using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Request.Conta
{
    public class CriarContaRequest
    {
        public string Nome { get; set; } = null!;
        public TipoConta Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
    }
}
