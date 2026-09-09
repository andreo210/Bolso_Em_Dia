using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class AporteMeta : IPertenceAoUsuario, IAuditoria
    {
        public int IdAporte { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdMeta { get; private set; }
        public int? IdTransacao { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Valor { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected AporteMeta() { } // EF

        // Só a MetaEconomia cria um aporte (ver MetaEconomia.RegistrarAporte).
        internal static AporteMeta Criar(string idUsuario, int idMeta, decimal valor, DateTime data, int? idTransacao)
        {
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            return new AporteMeta
            {
                IdUsuario = idUsuario,
                IdMeta = idMeta,
                IdTransacao = idTransacao,
                Data = data,
                Valor = valor
            };
        }
    }
}
