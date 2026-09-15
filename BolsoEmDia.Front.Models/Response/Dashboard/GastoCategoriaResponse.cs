namespace BolsoEmDia.Front.Models.Response.Dashboard
{
    public class GastoCategoriaResponse
    {
        public int IdCategoria { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentual { get; set; }
    }
}
