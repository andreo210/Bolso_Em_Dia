using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.FaturaServices
{
    public class FaturaService : IFaturaService
    {
        private readonly IFaturaRepository _faturaRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public FaturaService(
            IFaturaRepository faturaRepository,
            ICartaoRepository cartaoRepository,
            IContaRepository contaRepository,
            ICategoriaRepository categoriaRepository,
            ITransacaoRepository transacaoRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _faturaRepository = faturaRepository;
            _cartaoRepository = cartaoRepository;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
            _transacaoRepository = transacaoRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<FaturaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var fatura = await _faturaRepository.ObterPrimeiroAsync(
                f => f.IdFatura == id && f.IdUsuario == IdUsuarioAtual, ct: ct);
            return fatura?.ToDto();
        }

        public async Task<IReadOnlyList<FaturaDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var faturas = await _faturaRepository.ObterAsync(filtro: f => f.IdUsuario == IdUsuarioAtual, ct: ct);
            return faturas.ToDtoList();
        }

        // UC17 — Pagar fatura (inclui UC04 — mesma regra de saldo insuficiente de "Registrar despesa")
        public async Task<FaturaDto?> RegistrarPagamentoAsync(int idFatura, PagarFaturaDto dto, CancellationToken ct = default)
        {
            var fatura = await _faturaRepository.ObterPrimeiroAsync(
                f => f.IdFatura == idFatura && f.IdUsuario == IdUsuarioAtual, rastreado: true, ct: ct);
            if (fatura is null)
            {
                _notificador.Add("Fatura não encontrada");
                return null;
            }

            // E1 — nunca Paga; Aberta ou Fechada são os dois estados válidos (pagamento antecipado ou normal).
            if (fatura.Status == StatusFatura.Paga)
            {
                _notificador.Add("Fatura já está paga");
                return null;
            }

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == dto.IdCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (categoria is null)
                _notificador.Add("Categoria não encontrada");
            else if (categoria.Tipo != TipoCategoria.Despesa)
                _notificador.Add("Categoria informada não é uma categoria de despesa");
            else if (!categoria.Ativa)
                _notificador.Add("Categoria inativa");

            if (_notificador.TemNotificacao())
                return null;

            var cartao = await _cartaoRepository.ObterPrimeiroAsync(
                c => c.IdCartao == fatura.IdCartao && c.IdUsuario == IdUsuarioAtual, ct: ct);
            var contaPagamento = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == cartao!.IdContaPagamento && c.IdUsuario == IdUsuarioAtual, ct: ct);

            // E2 — UC05: mesma regra da despesa (Corrente sempre permite negativar).
            if (!contaPagamento!.PermiteSaldoNegativo())
            {
                var movimentacao = await _transacaoRepository.ObterSaldoAsync(contaPagamento.IdConta, DateTime.UtcNow.Date, ct);
                var saldoResultante = contaPagamento.SaldoInicial + movimentacao - fatura.ValorTotal;
                if (saldoResultante < 0)
                {
                    _notificador.Add("Saldo insuficiente na conta de pagamento");
                    return null;
                }
            }

            var faturaPaga = await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var transacaoPagamento = Transacao.RegistrarDespesa(
                    IdUsuarioAtual, contaPagamento.IdConta, dto.IdCategoria, DateTime.UtcNow, fatura.ValorTotal, "Pagamento fatura");

                // Precisa do Id real da transação antes de Fatura.RegistrarPagamento gravar
                // IdTransacaoPagamento — mesmo motivo de Parcela.IdFatura em Compra.Registrar.
                await _transacaoRepository.InserirAsync(transacaoPagamento, ct);
                await _transacaoRepository.SalvarAsync(ct);

                fatura.RegistrarPagamento(transacaoPagamento.IdTransacao);
                await _faturaRepository.AtualizarSalvarAsync(fatura, ct);

                return fatura;
            }, ct);

            return faturaPaga.ToDto();
        }
    }
}
