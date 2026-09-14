using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Response.Transacao
{
    public class TransacaoResponse
    {
        public int IdTransacao { get; set; }
        public int IdConta { get; set; }
        public int? IdCategoria { get; set; }
        public TipoTransacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }

        // Preenchido só quando a despesa estoura o orçamento da categoria no mês; nunca bloqueia.
        public string? AlertaOrcamento { get; set; }
    }
}
