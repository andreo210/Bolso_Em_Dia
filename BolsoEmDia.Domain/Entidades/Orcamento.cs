using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Orcamento : IPertenceAoUsuario, IAuditoria
    {
        public int IdOrcamento { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdCategoria { get; private set; }
        public DateOnly MesReferencia { get; private set; }
        public decimal ValorMeta { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Orcamento() { } // EF

        public static Orcamento Definir(string idUsuario, int idCategoria, DateOnly mesReferencia, decimal valorMeta)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (valorMeta <= 0)
                throw new DomainException("Valor da meta deve ser maior que zero");

            return new Orcamento
            {
                IdUsuario = idUsuario,
                IdCategoria = idCategoria,
                MesReferencia = new DateOnly(mesReferencia.Year, mesReferencia.Month, 1), // sempre 1º dia do mês
                ValorMeta = valorMeta
            };
        }

        public void AlterarMeta(decimal valorMeta)
        {
            if (valorMeta <= 0)
                throw new DomainException("Valor da meta deve ser maior que zero");

            ValorMeta = valorMeta;
        }

        // Estourar gera alerta — nunca bloqueia lançamento (ver regras-negocio-financas).
        public decimal PercentualConsumido(decimal totalGastoNoMes) => totalGastoNoMes / ValorMeta;

        public bool Estourado(decimal totalGastoNoMes) => totalGastoNoMes > ValorMeta;
    }
}
