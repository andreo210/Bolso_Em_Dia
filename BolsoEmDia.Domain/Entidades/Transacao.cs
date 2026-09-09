using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Transacao : IPertenceAoUsuario, IAuditoria
    {
        public int IdTransacao { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdConta { get; private set; }
        public int? IdCategoria { get; private set; }
        public int? IdTransferencia { get; private set; }
        public int? IdRecorrencia { get; private set; }
        public TipoTransacao Tipo { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Valor { get; private set; }
        public string? Descricao { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Transacao() { } // EF

        public static Transacao RegistrarReceita(
            string idUsuario, int idConta, int idCategoria, DateTime data, decimal valor, string? descricao, int? idRecorrencia = null)
            => Registrar(idUsuario, idConta, TipoTransacao.Receita, idCategoria, data, valor, descricao, idRecorrencia);

        public static Transacao RegistrarDespesa(
            string idUsuario, int idConta, int idCategoria, DateTime data, decimal valor, string? descricao, int? idRecorrencia = null)
            => Registrar(idUsuario, idConta, TipoTransacao.Despesa, idCategoria, data, valor, descricao, idRecorrencia);

        private static Transacao Registrar(
            string idUsuario, int idConta, TipoTransacao tipo, int idCategoria, DateTime data, decimal valor, string? descricao, int? idRecorrencia)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            return new Transacao
            {
                IdUsuario = idUsuario,
                IdConta = idConta,
                IdCategoria = idCategoria,
                IdRecorrencia = idRecorrencia,
                Tipo = tipo,
                Data = data,
                Valor = valor,
                Descricao = descricao
            };
        }

        // Só a Transferencia pode criar uma perna: garante que TransferenciaSaida/Entrada nunca
        // nasce solta, fora do par atômico (ver Transferencia.Registrar).
        internal static Transacao CriarPernaTransferencia(
            string idUsuario, int idTransferencia, int idConta, TipoTransacao tipo, DateTime data, decimal valor, string? descricao)
        {
            if (tipo != TipoTransacao.TransferenciaSaida && tipo != TipoTransacao.TransferenciaEntrada)
                throw new DomainException("Perna de transferência precisa ser do tipo TransferenciaSaida ou TransferenciaEntrada");
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            return new Transacao
            {
                IdUsuario = idUsuario,
                IdConta = idConta,
                IdTransferencia = idTransferencia,
                IdCategoria = null, // transferência nunca tem categoria
                Tipo = tipo,
                Data = data,
                Valor = valor,
                Descricao = descricao
            };
        }

        public void Editar(DateTime data, decimal valor, int? idCategoria, string? descricao)
        {
            if (Tipo == TipoTransacao.TransferenciaSaida || Tipo == TipoTransacao.TransferenciaEntrada)
                throw new DomainException("Perna de transferência não pode ser editada isoladamente — edite a Transferencia");
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            Data = data;
            Valor = valor;
            IdCategoria = idCategoria;
            Descricao = descricao;
        }

        // Transação com data futura é agendada: não entra no saldo atual, só no projetado.
        public bool EstaEfetivada(DateTime dataReferencia) => Data.Date <= dataReferencia.Date;
    }

    public enum TipoTransacao
    {
        Receita = 0,
        Despesa = 1,
        TransferenciaSaida = 2,
        TransferenciaEntrada = 3
    }
}
