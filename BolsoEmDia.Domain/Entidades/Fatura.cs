using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Fatura : IPertenceAoUsuario, IAuditoria
    {
        public int IdFatura { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdCartao { get; private set; }
        public int? IdTransacaoPagamento { get; private set; }
        public DateOnly MesReferencia { get; private set; }
        public DateTime DataFechamento { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public decimal ValorTotal { get; private set; }
        public StatusFatura Status { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Fatura() { } // EF

        // Só o Cartao abre fatura (ver Cartao.AbrirFatura).
        internal static Fatura Abrir(string idUsuario, int idCartao, DateOnly mesReferencia, DateTime dataFechamento, DateTime dataVencimento)
        {
            return new Fatura
            {
                IdUsuario = idUsuario,
                IdCartao = idCartao,
                MesReferencia = mesReferencia,
                DataFechamento = dataFechamento,
                DataVencimento = dataVencimento,
                ValorTotal = 0,
                Status = StatusFatura.Aberta
            };
        }

        public void Fechar()
        {
            if (Status != StatusFatura.Aberta)
                throw new DomainException("Somente fatura aberta pode ser fechada");

            Status = StatusFatura.Fechada;
        }

        // Aberta -> Paga é pagamento antecipado (usuário quita antes do fechamento); Fechada -> Paga
        // é o fluxo normal. Paga é terminal — ver diagrama-estados.md.
        public void RegistrarPagamento(int idTransacaoPagamento)
        {
            if (Status == StatusFatura.Paga)
                throw new DomainException("Fatura já está paga");

            IdTransacaoPagamento = idTransacaoPagamento;
            Status = StatusFatura.Paga;
        }

        // Denormalizado: recalculado a cada escrita em Parcela dessa fatura, inclusive
        // estorno de compra (que pode reduzir o total de uma fatura já fechada, mas não paga).
        public void RecalcularValorTotal(decimal somaParcelas)
        {
            if (somaParcelas < 0)
                throw new DomainException("Valor total da fatura não pode ser negativo");

            ValorTotal = somaParcelas;
        }

        public bool AceitaNovoLancamento() => Status == StatusFatura.Aberta;
    }

    public enum StatusFatura
    {
        Aberta = 0,
        Fechada = 1,
        Paga = 2
    }
}
