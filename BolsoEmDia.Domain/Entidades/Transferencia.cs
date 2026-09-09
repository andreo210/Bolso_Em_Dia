using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Transferencia : IPertenceAoUsuario, IAuditoria
    {
        private readonly List<Transacao> _pernas = new();

        public int IdTransferencia { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdContaOrigem { get; private set; }
        public int IdContaDestino { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }
        public IReadOnlyCollection<Transacao> Pernas => _pernas;

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Transferencia() { } // EF

        public static Transferencia Registrar(
            string idUsuario, int idContaOrigem, int idContaDestino, DateTime data, decimal valor, string? descricao)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (idContaOrigem == idContaDestino)
                throw new DomainException("Conta de origem e destino não podem ser a mesma");
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            var transferencia = new Transferencia
            {
                IdUsuario = idUsuario,
                IdContaOrigem = idContaOrigem,
                IdContaDestino = idContaDestino,
                Data = data,
                Valor = valor,
                Descricao = descricao
            };

            // as duas pernas nascem juntas aqui dentro — nunca é possível existir uma sem a outra
            transferencia._pernas.Add(Transacao.CriarPernaTransferencia(
                idUsuario, transferencia.IdTransferencia, idContaOrigem, TipoTransacao.TransferenciaSaida, data, valor, descricao));
            transferencia._pernas.Add(Transacao.CriarPernaTransferencia(
                idUsuario, transferencia.IdTransferencia, idContaDestino, TipoTransacao.TransferenciaEntrada, data, valor, descricao));

            return transferencia;
        }
    }
}
