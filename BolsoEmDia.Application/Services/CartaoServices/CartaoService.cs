using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.CartaoServices
{
    public class CartaoService : ICartaoService
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public CartaoService(
            ICartaoRepository cartaoRepository,
            IContaRepository contaRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _cartaoRepository = cartaoRepository;
            _contaRepository = contaRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<CartaoDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var cartao = await _cartaoRepository.ObterPrimeiroAsync(
                c => c.IdCartao == id && c.IdUsuario == IdUsuarioAtual, rastreado: false, ct: ct);
            return cartao?.ToDto();
        }

        public async Task<IReadOnlyList<CartaoDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var cartoes = await _cartaoRepository.ObterAsync(filtro: c => c.IdUsuario == IdUsuarioAtual, ct: ct);
            return cartoes.ToDtoList();
        }

        // UC13 — Cadastrar cartão de crédito
        public async Task<CartaoDto?> CriarAsync(CriarCartaoDto dto, CancellationToken ct = default)
        {
            // Pré-condição: conta de pagamento precisa existir e pertencer ao usuário.
            var contaExiste = await _contaRepository.ExisteAsync(
                c => c.IdConta == dto.IdContaPagamento && c.IdUsuario == IdUsuarioAtual, ct);
            if (!contaExiste)
            {
                _notificador.Add("Conta de pagamento não encontrada");
                return null;
            }

            var cartao = Cartao.Criar(
                IdUsuarioAtual, dto.Nome, dto.LimiteTotal, dto.DiaFechamento, dto.DiaVencimento, dto.IdContaPagamento);
            var salvo = await _cartaoRepository.InserirSalvarAsync(cartao, ct);
            return salvo.ToDto();
        }
    }
}
