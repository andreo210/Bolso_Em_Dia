using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Conta : IPertenceAoUsuario, IAuditoria
    {
        public int IdConta { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public string Nome { get; private set; } = null!;
        public TipoConta Tipo { get; private set; }
        public decimal SaldoInicial { get; private set; }
        public bool Ativa { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Conta() { } // EF

        public static Conta Criar(string idUsuario, string nome, TipoConta tipo, decimal saldoInicial)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");
            // saldo inicial negativo só é possível em conta corrente (cheque especial)
            if (saldoInicial < 0 && tipo != TipoConta.Corrente)
                throw new DomainException("Somente conta corrente pode ter saldo inicial negativo");

            return new Conta
            {
                IdUsuario = idUsuario,
                Nome = nome,
                Tipo = tipo,
                SaldoInicial = saldoInicial,
                Ativa = true
            };
        }

        public void Renomear(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");

            Nome = nome;
        }

        public void Ativar() => Ativa = true;

        public void Desativar() => Ativa = false;

        // Corrente aceita saldo negativo (cheque especial); poupança, carteira e investimento não.
        public bool PermiteSaldoNegativo() => Tipo == TipoConta.Corrente;
    }

    public enum TipoConta
    {
        Corrente = 0,
        Poupanca = 1,
        Carteira = 2,
        Investimento = 3
    }
}
