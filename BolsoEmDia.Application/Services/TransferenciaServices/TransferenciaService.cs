using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.TransferenciaServices
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IContaRepository _contaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public TransferenciaService(
            ITransferenciaRepository transferenciaRepository,
            ITransacaoRepository transacaoRepository,
            IContaRepository contaRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _transferenciaRepository = transferenciaRepository;
            _transacaoRepository = transacaoRepository;
            _contaRepository = contaRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        // UC06 — Transferir entre contas (inclui UC05)
        public async Task<TransferenciaDto?> RegistrarAsync(CriarTransferenciaDto dto, CancellationToken ct = default)
        {
            var contaOrigem = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == dto.IdContaOrigem && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (contaOrigem is null)
                _notificador.Add("Conta de origem não encontrada");

            var contaDestino = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == dto.IdContaDestino && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (contaDestino is null)
                _notificador.Add("Conta de destino não encontrada");

            if (dto.IdContaOrigem == dto.IdContaDestino)
                _notificador.Add("Conta de origem e destino não podem ser a mesma");

            if (_notificador.TemNotificacao())
                return null;

            // UC05 — Verificar saldo da conta: a perna de saída segue a mesma regra de uma despesa.
            if (!contaOrigem!.PermiteSaldoNegativo())
            {
                var movimentacao = await _transacaoRepository.ObterSaldoAsync(contaOrigem.IdConta, DateTime.UtcNow.Date, ct);
                var saldoResultante = contaOrigem.SaldoInicial + movimentacao - dto.Valor;
                if (saldoResultante < 0)
                {
                    _notificador.Add("Saldo insuficiente na conta de origem");
                    return null;
                }
            }

            var transferencia = await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var novaTransferencia = Transferencia.Registrar(
                    IdUsuarioAtual, dto.IdContaOrigem, dto.IdContaDestino, dto.Data, dto.Valor, dto.Descricao);

                await _transferenciaRepository.InserirAsync(novaTransferencia, ct);
                foreach (var perna in novaTransferencia.Pernas)
                    await _transacaoRepository.InserirAsync(perna, ct);

                return novaTransferencia;
            }, ct);

            return transferencia.ToDto();
        }
    }
}
