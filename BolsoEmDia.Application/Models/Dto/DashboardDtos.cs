namespace BolsoEmDia.Application.Models.Dto
{
    public class DashboardDto
    {
        public DateOnly MesReferencia { get; set; }
        public decimal TotalReceitasMes { get; set; }
        public decimal TotalDespesasMes { get; set; }
        public decimal SaldoMes { get; set; }
        public List<GastoCategoriaDto> GastosPorCategoria { get; set; } = new();
        public List<EvolucaoMensalDto> EvolucaoMensal { get; set; } = new();
    }

    public class GastoCategoriaDto
    {
        public int IdCategoria { get; set; }
        public string NomeCategoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentual { get; set; }
    }

    public class EvolucaoMensalDto
    {
        public DateOnly Mes { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
    }
}
