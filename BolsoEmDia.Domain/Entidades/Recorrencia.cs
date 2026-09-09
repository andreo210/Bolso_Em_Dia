using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Recorrencia : IPertenceAoUsuario, IAuditoria
    {
        public int IdRecorrencia { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int? IdConta { get; private set; }
        public int? IdCartao { get; private set; }
        public int IdCategoria { get; private set; }
        public TipoTransacao? TipoTransacao { get; private set; }
        public decimal Valor { get; private set; }
        public FrequenciaRecorrencia Frequencia { get; private set; }
        public int DiaGeracao { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public bool Ativa { get; private set; }

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Recorrencia() { } // EF

        public static Recorrencia Criar(
            string idUsuario, int? idConta, int? idCartao, int idCategoria, TipoTransacao? tipoTransacao,
            decimal valor, FrequenciaRecorrencia frequencia, int diaGeracao, DateTime dataInicio, DateTime? dataFim = null)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            // exatamente um entre IdConta e IdCartao, nunca os dois, nunca nenhum
            if ((idConta is null) == (idCartao is null))
                throw new DomainException("Recorrência deve estar ligada a exatamente uma conta ou um cartão");
            if (idConta is not null && tipoTransacao is (BolsoEmDia.Domain.Entidades.TipoTransacao.TransferenciaSaida or BolsoEmDia.Domain.Entidades.TipoTransacao.TransferenciaEntrada))
                throw new DomainException("Recorrência em conta não pode gerar transferência");
            if (idConta is not null && tipoTransacao is null)
                throw new DomainException("Tipo da transação é obrigatório quando a recorrência gera lançamento em conta");
            if (valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");
            if (dataFim.HasValue && dataFim.Value.Date < dataInicio.Date)
                throw new DomainException("Data fim não pode ser anterior à data de início");
            ValidarDiaGeracao(frequencia, diaGeracao);

            return new Recorrencia
            {
                IdUsuario = idUsuario,
                IdConta = idConta,
                IdCartao = idCartao,
                IdCategoria = idCategoria,
                TipoTransacao = idConta is not null ? tipoTransacao : null,
                Valor = valor,
                Frequencia = frequencia,
                DiaGeracao = diaGeracao,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Ativa = true
            };
        }

        private static void ValidarDiaGeracao(FrequenciaRecorrencia frequencia, int diaGeracao)
        {
            var valido = frequencia == FrequenciaRecorrencia.Semanal
                ? diaGeracao is >= 0 and <= 6
                : diaGeracao is >= 1 and <= 31;

            if (!valido)
                throw new DomainException("Dia de geração inválido para a frequência informada");
        }

        public void Pausar() => Ativa = false;

        public void Reativar() => Ativa = true;

        // Se o dia não existir no mês (ex.: dia 31 num mês de 30 dias), gera no último dia do mês.
        public DateTime ProximaDataGeracao(DateTime dataReferencia)
        {
            var referencia = dataReferencia.Date;

            return Frequencia switch
            {
                FrequenciaRecorrencia.Semanal => ProximaOcorrenciaSemanal(referencia),
                FrequenciaRecorrencia.Mensal => ProximaOcorrenciaMensal(referencia),
                FrequenciaRecorrencia.Anual => ProximaOcorrenciaAnual(referencia),
                _ => throw new DomainException("Frequência de recorrência inválida")
            };
        }

        private DateTime ProximaOcorrenciaSemanal(DateTime referencia)
        {
            var diaAlvo = (DayOfWeek)DiaGeracao;
            var diasParaSomar = ((int)diaAlvo - (int)referencia.DayOfWeek + 7) % 7;
            diasParaSomar = diasParaSomar == 0 ? 7 : diasParaSomar;
            return referencia.AddDays(diasParaSomar);
        }

        private DateTime ProximaOcorrenciaMensal(DateTime referencia)
        {
            var candidata = DiaEfetivoNoMes(referencia.Year, referencia.Month);
            return candidata > referencia ? candidata : DiaEfetivoNoMes(referencia.AddMonths(1).Year, referencia.AddMonths(1).Month);
        }

        private DateTime ProximaOcorrenciaAnual(DateTime referencia)
        {
            var candidata = DiaEfetivoNoMes(referencia.Year, DataInicio.Month);
            return candidata > referencia ? candidata : DiaEfetivoNoMes(referencia.Year + 1, DataInicio.Month);
        }

        private DateTime DiaEfetivoNoMes(int ano, int mes)
        {
            var dia = Math.Min(DiaGeracao, DateTime.DaysInMonth(ano, mes));
            return new DateTime(ano, mes, dia, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// A Recorrencia não guarda referência às raízes Conta/Cartao, então quem de fato cria a
        /// Transacao (via RegistrarReceita/RegistrarDespesa) ou a Compra (via Compra.Registrar,
        /// que precisa do Cartao) é o serviço de aplicação. Este método só valida se a geração
        /// ainda é permitida e desativa o modelo quando o prazo final passou.
        /// </summary>
        public void GerarOcorrencia(DateTime dataReferencia)
        {
            if (!Ativa)
                throw new DomainException("Recorrência pausada não gera novas ocorrências");
            if (DataFim.HasValue && dataReferencia.Date > DataFim.Value.Date)
            {
                Pausar();
                throw new DomainException("Recorrência já passou da data de término");
            }
        }
    }

    public enum FrequenciaRecorrencia
    {
        Semanal = 0,
        Mensal = 1,
        Anual = 2
    }
}
