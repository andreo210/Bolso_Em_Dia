namespace BolsoEmDia.Front.Models.Response.Dashboard
{
    public class DashboardResponse
    {
        public DateOnly MesReferencia { get; set; }
        public decimal TotalReceitasMes { get; set; }
        public decimal TotalDespesasMes { get; set; }
        public decimal SaldoMes { get; set; }
        public List<GastoCategoriaResponse> GastosPorCategoria { get; set; } = new();
        public List<EvolucaoMensalResponse> EvolucaoMensal { get; set; } = new();
    }
}
