using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Cartao : IPertenceAoUsuario, IAuditoria
    {
        private readonly List<Fatura> _faturas = new();

        public int IdCartao { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdContaPagamento { get; private set; }
        public string Nome { get; private set; } = null!;
        public decimal LimiteTotal { get; private set; }
        public int DiaFechamento { get; private set; }
        public int DiaVencimento { get; private set; }
        public bool Ativo { get; private set; }
        public IReadOnlyCollection<Fatura> Faturas => _faturas;

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Cartao() { } // EF

        public static Cartao Criar(
            string idUsuario, string nome, decimal limiteTotal, int diaFechamento, int diaVencimento, int idContaPagamento)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");
            if (limiteTotal <= 0)
                throw new DomainException("Limite total deve ser maior que zero");
            if (diaFechamento is < 1 or > 31)
                throw new DomainException("Dia de fechamento deve estar entre 1 e 31");
            if (diaVencimento is < 1 or > 31)
                throw new DomainException("Dia de vencimento deve estar entre 1 e 31");

            return new Cartao
            {
                IdUsuario = idUsuario,
                Nome = nome,
                LimiteTotal = limiteTotal,
                DiaFechamento = diaFechamento,
                DiaVencimento = diaVencimento,
                IdContaPagamento = idContaPagamento,
                Ativo = true
            };
        }

        // Limite disponível = limite total − soma de TODAS as parcelas futuras não pagas
        // (não só a fatura aberta). Quem soma as parcelas é o serviço/repositório.
        public decimal LimiteDisponivel(decimal totalParcelasFuturasNaoPagas) => LimiteTotal - totalParcelasFuturasNaoPagas;

        public Fatura? FaturaAbertaPara(DateTime data)
        {
            var mesReferencia = CalcularMesReferencia(data);
            return _faturas.FirstOrDefault(f => f.MesReferencia == mesReferencia && f.Status == StatusFatura.Aberta);
        }

        // Só o Cartao abre fatura — encapsula Fatura.Abrir (internal).
        public Fatura AbrirFatura(DateOnly mesReferencia, DateTime dataFechamento, DateTime dataVencimento)
        {
            var fatura = Fatura.Abrir(IdUsuario, IdCartao, mesReferencia, dataFechamento, dataVencimento);
            _faturas.Add(fatura);
            return fatura;
        }

        // Fica no Cartao (não em Compra) porque só ele conhece seu próprio estado de faturas —
        // Compra.Registrar chama isso uma vez por parcela. Fatura fechada é imutável: se o ciclo
        // alvo já fechou, a parcela desliza para o próximo ciclo em aberto em vez de entrar nela.
        //
        // Idempotente: chamar de novo para o mesmo mês (já resolvido) devolve a mesma instância,
        // sem abrir fatura duplicada — é o que permite ao CompraService resolver e persistir as
        // faturas de todos os ciclos ANTES de montar as parcelas (ver comentário em Compra.Registrar
        // sobre Parcela.IdFatura precisar de um Id já gerado).
        public Fatura ObterOuAbrirFaturaParaLancamento(DateOnly mesReferenciaDesejado)
        {
            var mesReferencia = mesReferenciaDesejado;

            while (true)
            {
                var fatura = _faturas.FirstOrDefault(f => f.MesReferencia == mesReferencia);

                if (fatura == null)
                    return AbrirFatura(mesReferencia, CalcularDataFechamento(mesReferencia), CalcularDataVencimento(mesReferencia));

                if (fatura.AceitaNovoLancamento())
                    return fatura;

                mesReferencia = mesReferencia.AddMonths(1);
            }
        }

        // Compra até o dia de fechamento entra no ciclo corrente; depois, no ciclo seguinte.
        public DateOnly CalcularMesReferencia(DateTime data)
        {
            var diaFechamentoEfetivo = Math.Min(DiaFechamento, DateTime.DaysInMonth(data.Year, data.Month));
            var primeiroDiaDoMes = new DateOnly(data.Year, data.Month, 1);

            return data.Day <= diaFechamentoEfetivo ? primeiroDiaDoMes : primeiroDiaDoMes.AddMonths(1);
        }

        public DateTime CalcularDataFechamento(DateOnly mesReferencia)
        {
            var dia = Math.Min(DiaFechamento, DateTime.DaysInMonth(mesReferencia.Year, mesReferencia.Month));
            return new DateTime(mesReferencia.Year, mesReferencia.Month, dia, 0, 0, 0, DateTimeKind.Utc);
        }

        // Vencimento vem depois do fechamento: se o dia de vencimento é menor ou igual ao de
        // fechamento, o vencimento cai no mês seguinte (ex.: fecha dia 28, vence dia 5).
        public DateTime CalcularDataVencimento(DateOnly mesReferencia)
        {
            var mesVencimento = DiaVencimento <= DiaFechamento ? mesReferencia.AddMonths(1) : mesReferencia;
            var dia = Math.Min(DiaVencimento, DateTime.DaysInMonth(mesVencimento.Year, mesVencimento.Month));
            return new DateTime(mesVencimento.Year, mesVencimento.Month, dia, 0, 0, 0, DateTimeKind.Utc);
        }

        public void Ativar() => Ativo = true;

        public void Desativar() => Ativo = false;
    }
}
