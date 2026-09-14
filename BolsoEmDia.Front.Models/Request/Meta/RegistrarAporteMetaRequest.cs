namespace BolsoEmDia.Front.Models.Request.Meta
{
    public class RegistrarAporteMetaRequest
    {
        public decimal Valor { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
        public int? IdTransacao { get; set; }
    }
}
