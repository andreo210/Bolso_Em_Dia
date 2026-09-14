namespace BolsoEmDia.Front.Models.Response.Meta
{
    public class MetaEconomiaResponse
    {
        public int IdMeta { get; set; }
        public int? IdConta { get; set; }
        public string Nome { get; set; } = null!;
        public decimal ValorAlvo { get; set; }
        public DateTime? DataAlvo { get; set; }
        public bool Concluida { get; set; }
        public decimal TotalAportado { get; set; }
    }
}
