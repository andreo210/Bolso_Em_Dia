namespace BolsoEmDia.Front.Models.Response.Meta
{
    public class AporteMetaResponse
    {
        public int IdAporte { get; set; }
        public int IdMeta { get; set; }
        public int? IdTransacao { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
    }
}
