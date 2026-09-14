namespace BolsoEmDia.Front.Models.Request.Meta
{
    public class CriarMetaEconomiaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public decimal ValorAlvo { get; set; }
        public DateTime? DataAlvo { get; set; }
        public int? IdConta { get; set; }
    }
}
