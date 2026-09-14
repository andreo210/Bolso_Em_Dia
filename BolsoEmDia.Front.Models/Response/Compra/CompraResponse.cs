namespace BolsoEmDia.Front.Models.Response.Compra
{
    public class ParcelaResponse
    {
        public int IdParcela { get; set; }
        public int IdFatura { get; set; }
        public int Numero { get; set; }
        public decimal Valor { get; set; }
    }

    public class CompraResponse
    {
        public int IdCompra { get; set; }
        public int IdCartao { get; set; }
        public int IdCategoria { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = null!;
        public decimal ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public List<ParcelaResponse> Parcelas { get; set; } = new();
    }
}
