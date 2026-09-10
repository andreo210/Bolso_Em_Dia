using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Application.Services.OrcamentoServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.TransacaoServices
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IOrcamentoService _orcamentoService;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public TransacaoService(
            ITransacaoRepository transacaoRepository,
            IContaRepository contaRepository,
            ICategoriaRepository categoriaRepository,
            IOrcamentoService orcamentoService,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _transacaoRepository = transacaoRepository;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
            _orcamentoService = orcamentoService;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        // UC03 — Registrar receita
        public async Task<TransacaoDto?> RegistrarReceitaAsync(CriarReceitaDto dto, CancellationToken ct = default)
        {
            var conta = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == dto.IdConta && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (conta is null)
                _notificador.Add("Conta não encontrada");

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == dto.IdCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (categoria is null)
                _notificador.Add("Categoria não encontrada");
            else if (categoria.Tipo != TipoCategoria.Receita)
                _notificador.Add("Categoria informada não é uma categoria de receita");

            if (_notificador.TemNotificacao())
                return null;

            var transacao = Transacao.RegistrarReceita(
                IdUsuarioAtual, dto.IdConta, dto.IdCategoria, dto.Data, dto.Valor, dto.Descricao);

            var salva = await _transacaoRepository.InserirSalvarAsync(transacao, ct);
            return salva.ToDto();
        }

        // UC04 — Registrar despesa (inclui UC05, estende UC10)
        public async Task<TransacaoDto?> RegistrarDespesaAsync(CriarDespesaDto dto, CancellationToken ct = default)
        {
            var conta = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == dto.IdConta && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (conta is null)
                _notificador.Add("Conta não encontrada");

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == dto.IdCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (categoria is null)
                _notificador.Add("Categoria não encontrada");
            else if (categoria.Tipo != TipoCategoria.Despesa)
                _notificador.Add("Categoria informada não é uma categoria de despesa");

            if (_notificador.TemNotificacao())
                return null;

            // UC05 — Verificar saldo da conta: sempre acontece, nunca é opcional.
            if (!conta!.PermiteSaldoNegativo())
            {
                var movimentacao = await _transacaoRepository.ObterSaldoAsync(conta.IdConta, DateTime.UtcNow.Date, ct);
                var saldoResultante = conta.SaldoInicial + movimentacao - dto.Valor;
                if (saldoResultante < 0)
                {
                    _notificador.Add("Saldo insuficiente");
                    return null;
                }
            }

            var transacao = Transacao.RegistrarDespesa(
                IdUsuarioAtual, dto.IdConta, dto.IdCategoria, dto.Data, dto.Valor, dto.Descricao);

            var salva = await _transacaoRepository.InserirSalvarAsync(transacao, ct);

            // UC10 — checagem de orçamento acontece depois de persistir: o alerta nunca bloqueia o passo anterior.
            var mesReferencia = new DateOnly(dto.Data.Year, dto.Data.Month, 1);
            var alertaOrcamento = await _orcamentoService.VerificarEstouroAsync(dto.IdCategoria, mesReferencia, ct);

            var resultado = salva.ToDto();
            resultado.AlertaOrcamento = alertaOrcamento;
            return resultado;
        }
    }
}
