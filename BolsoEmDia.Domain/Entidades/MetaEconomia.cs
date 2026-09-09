using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class MetaEconomia : IPertenceAoUsuario, IAuditoria
    {
        private readonly List<AporteMeta> _aportes = new();

        public int IdMeta { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int? IdConta { get; private set; }
        public string Nome { get; private set; } = null!;
        public decimal ValorAlvo { get; private set; }
        public DateTime? DataAlvo { get; private set; }
        public bool Concluida { get; private set; }
        public IReadOnlyCollection<AporteMeta> Aportes => _aportes;

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected MetaEconomia() { } // EF

        public static MetaEconomia Criar(string idUsuario, string nome, decimal valorAlvo, DateTime? dataAlvo, int? idConta)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");
            if (valorAlvo <= 0)
                throw new DomainException("Valor alvo deve ser maior que zero");

            return new MetaEconomia
            {
                IdUsuario = idUsuario,
                Nome = nome,
                ValorAlvo = valorAlvo,
                DataAlvo = dataAlvo,
                IdConta = idConta,
                Concluida = false
            };
        }

        // A meta não valida saldo — quem bloqueia (ou não) é a conta de origem do aporte.
        public void RegistrarAporte(decimal valor, DateTime data, int? idTransacao)
        {
            _aportes.Add(AporteMeta.Criar(IdUsuario, IdMeta, valor, data, idTransacao));
            VerificarConclusao();
        }

        public decimal TotalAportado() => _aportes.Sum(a => a.Valor);

        public void VerificarConclusao() => Concluida = TotalAportado() >= ValorAlvo;
    }
}
