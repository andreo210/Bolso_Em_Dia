using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Parcela : IPertenceAoUsuario, IAuditoria
    {
        public int IdParcela { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdCompra { get; private set; }
        public int IdFatura { get; private set; }
        public int Numero { get; private set; }
        public decimal Valor { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Parcela() { } // EF

        // Só a Compra cria parcela (ver Compra.Registrar).
        internal static Parcela Criar(string idUsuario, int idCompra, int idFatura, int numero, decimal valor)
        {
            if (numero < 1)
                throw new DomainException("Número da parcela deve ser maior ou igual a 1");
            if (valor <= 0)
                throw new DomainException("Valor da parcela deve ser maior que zero");

            return new Parcela
            {
                IdUsuario = idUsuario,
                IdCompra = idCompra,
                IdFatura = idFatura,
                Numero = numero,
                Valor = valor
            };
        }
    }
}
