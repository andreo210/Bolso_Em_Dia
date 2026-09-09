
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.ContaServices
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public ContaService(
            IContaRepository contaRepository,
            ITransacaoRepository transacaoRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _contaRepository = contaRepository;
            _transacaoRepository = transacaoRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<ContaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var conta = await ObterContaDoUsuarioAsync(id, rastreado: false, ct);
            return conta?.ToDto();
        }

        public async Task<IReadOnlyList<ContaDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var contas = await _contaRepository.ObterAsync(c => c.IdUsuario == IdUsuarioAtual, ct: ct);
            return contas.ToDtoList();
        }

        // UC05 — saldo = saldo inicial + soma das transações já efetivadas (data <= hoje);
        // agendamentos futuros não entram (ver regras-negocio-financas).
        public async Task<SaldoContaDto?> ObterSaldoAsync(int id, CancellationToken ct = default)
        {
            var conta = await ObterContaDoUsuarioAsync(id, rastreado: false, ct);
            if (conta is null) return null;

            var hoje = DateTime.UtcNow.Date;
            var transacoes = await _transacaoRepository.ObterAsync(
                filtro: t => t.IdConta == id && t.Data.Date <= hoje,
                ct: ct);

            var movimentacao = transacoes.Sum(t =>
                t.Tipo is TipoTransacao.Despesa or TipoTransacao.TransferenciaSaida ? -t.Valor : t.Valor);

            return new SaldoContaDto
            {
                IdConta = conta.IdConta,
                Saldo = conta.SaldoInicial + movimentacao,
                DataReferencia = hoje
            };
        }

        public async Task<ContaDto?> CriarAsync(CriarContaDto dto, CancellationToken ct = default)
        {
            if (dto.SaldoInicial < 0 && dto.Tipo != TipoConta.Corrente)
            {
                _notificador.Add("Somente conta corrente pode ter saldo inicial negativo");
                return null;
            }

            var conta = Conta.Criar(IdUsuarioAtual, dto.Nome, dto.Tipo, dto.SaldoInicial);
            var salva = await _contaRepository.InserirSalvarAsync(conta, ct);
            return salva.ToDto();
        }

        public async Task<bool> AtualizarAsync(int id, AtualizarContaDto dto, CancellationToken ct = default)
        {
            var conta = await ObterContaDoUsuarioAsync(id, rastreado: true, ct);
            if (conta is null)
            {
                _notificador.Add("Conta não encontrada");
                return false;
            }

            conta.Renomear(dto.Nome);
            return await _contaRepository.AtualizarSalvarAsync(conta, ct);
        }

        public async Task<bool> AtivarAsync(int id, CancellationToken ct = default)
            => await AlterarSituacaoAsync(id, ativar: true, ct);

        public async Task<bool> InativarAsync(int id, CancellationToken ct = default)
            => await AlterarSituacaoAsync(id, ativar: false, ct);

        private async Task<bool> AlterarSituacaoAsync(int id, bool ativar, CancellationToken ct)
        {
            var conta = await ObterContaDoUsuarioAsync(id, rastreado: true, ct);
            if (conta is null)
            {
                _notificador.Add("Conta não encontrada");
                return false;
            }

            if (ativar) conta.Ativar();
            else conta.Desativar();

            return await _contaRepository.AtualizarSalvarAsync(conta, ct);
        }

        // Escopa toda leitura/escrita pelo usuário logado: conta de outro usuário nunca aparece,
        // nem para confirmar que existe.
        private Task<Conta?> ObterContaDoUsuarioAsync(int id, bool rastreado, CancellationToken ct)
            => _contaRepository.ObterPrimeiroAsync(c => c.IdConta == id && c.IdUsuario == IdUsuarioAtual, rastreado: rastreado, ct: ct);
    }
}
