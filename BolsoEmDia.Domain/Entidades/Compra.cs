using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Compra : IPertenceAoUsuario, IAuditoria
    {
        private readonly List<Parcela> _parcelas = new();

        public int IdCompra { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public int IdCartao { get; private set; }
        public int IdCategoria { get; private set; }
        public int? IdRecorrencia { get; private set; }
        public DateTime Data { get; private set; }
        public string Descricao { get; private set; } = null!;
        public decimal ValorTotal { get; private set; }
        public int NumeroParcelas { get; private set; }
        public IReadOnlyCollection<Parcela> Parcelas => _parcelas;

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Compra() { } // EF

        /// <summary>
        /// Recebe o próprio <see cref="Cartao"/> (não só o id) porque resolver em qual fatura
        /// cada parcela cai — e abrir uma fatura nova quando não existe uma para o ciclo — é
        /// uma regra que só o agregado Cartao sabe aplicar (ver Cartao.CalcularMesReferencia/
        /// ObterOuAbrirFaturaParaLancamento). É um toque deliberado entre agregados, documentado
        /// aqui como pede a convenção de "Quando um agregado toca o outro" (arquitetura-api/dominio.md).
        ///
        /// Parcela.IdFatura é uma referência entre agregados por id (sem navegação EF, de
        /// propósito — Parcela não pertence ao agregado Cartao). Por isso quem chama isto
        /// precisa garantir que toda fatura que vai ser atingida já tem Id real gravado —
        /// senão uma fatura aberta agora mesmo (Id ainda 0) grava 0 na coluna, não o Id de
        /// verdade. Ver CompraService.RegistrarAsync: ele resolve/abre e salva as faturas dos N
        /// ciclos ANTES de chamar este método.
        /// </summary>
        public static Compra Registrar(
            string idUsuario, Cartao cartao, int idCategoria, DateTime data, string descricao,
            decimal valorTotal, int numeroParcelas, decimal limiteDisponivel, int? idRecorrencia = null)
        {
            ArgumentNullException.ThrowIfNull(cartao);

            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("Descrição é obrigatória");
            if (valorTotal <= 0)
                throw new DomainException("Valor total deve ser maior que zero");
            if (numeroParcelas < 1)
                throw new DomainException("Número de parcelas deve ser maior ou igual a 1");
            // bloqueia a compra inteira — nunca grava parte das parcelas
            if (valorTotal > limiteDisponivel)
                throw new DomainException("Compra excede o limite disponível do cartão");

            var compra = new Compra
            {
                IdUsuario = idUsuario,
                IdCartao = cartao.IdCartao,
                IdCategoria = idCategoria,
                IdRecorrencia = idRecorrencia,
                Data = data,
                Descricao = descricao,
                ValorTotal = valorTotal,
                NumeroParcelas = numeroParcelas
            };

            var valorParcela = Math.Round(valorTotal / numeroParcelas, 2, MidpointRounding.ToEven);
            var mesReferenciaBase = cartao.CalcularMesReferencia(data);

            for (var numero = 1; numero <= numeroParcelas; numero++)
            {
                // a última parcela absorve o resto do arredondamento para a soma bater exatamente
                var valor = numero == numeroParcelas
                    ? valorTotal - valorParcela * (numeroParcelas - 1)
                    : valorParcela;

                var fatura = cartao.ObterOuAbrirFaturaParaLancamento(mesReferenciaBase.AddMonths(numero - 1));
                compra._parcelas.Add(Parcela.Criar(idUsuario, compra.IdCompra, fatura.IdFatura, numero, valor));
            }

            return compra;
        }

        // Remove as parcelas futuras e libera o limite total da compra. Estorno parcial (com
        // parcelas já pagas) não é modelado por padrão — ver regras-negocio-financas/cartao-de-credito.md.
        public void Cancelar() => _parcelas.Clear();
    }
}
