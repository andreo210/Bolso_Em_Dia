namespace BolsoEmDia.Front.Models.Response.Dashboard
{
    public class EvolucaoMensalResponse
    {
        public DateOnly Mes { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
    }
}
