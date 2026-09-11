using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BolsoEmDia.Application.Services.RecorrenciaServices
{
    /// <summary>
    /// UC21 — Gerar ocorrência de recorrência. Roda dentro do escopo aberto pelo RecorrenciaJob
    /// (hosted service, em BolsoEmDia.Api).
    ///
    /// Não reaproveita ITransacaoService/ICompraService: os dois leem o usuário atual de
    /// ICurrentUser, que depende de HttpContext — inexistente aqui, já que uma única execução do
    /// job atravessa recorrências de vários usuários. Em vez disso replica localmente a mesma
    /// checagem de saldo/limite que UC04/UC14 já fazem, usando Recorrencia.IdUsuario — mesmo
    /// caminho de FaturaService.RegistrarPagamentoAsync para UC17→UC04.
    /// </summary>
    public class RecorrenciaJobService : IRecorrenciaJobService
    {
        private readonly IRecorrenciaRepository _recorrenciaRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly ICompraRepository _compraRepository;
        private readonly IParcelaRepository _parcelaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecorrenciaJobService> _logger;

        public RecorrenciaJobService(
            IRecorrenciaRepository recorrenciaRepository,
            IContaRepository contaRepository,
            ICategoriaRepository categoriaRepository,
            ITransacaoRepository transacaoRepository,
            ICartaoRepository cartaoRepository,
            ICompraRepository compraRepository,
            IParcelaRepository parcelaRepository,
            IUnitOfWork unitOfWork,
            ILogger<RecorrenciaJobService> logger)
        {
            _recorrenciaRepository = recorrenciaRepository;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
            _transacaoRepository = transacaoRepository;
            _cartaoRepository = cartaoRepository;
            _compraRepository = compraRepository;
            _parcelaRepository = parcelaRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessarGeracoesAsync(DateTime dataReferencia, CancellationToken ct = default)
        {
            var recorrenciasAtivas = await _recorrenciaRepository.ObterAsync(filtro: r => r.Ativa, rastreado: true, ct: ct);

            foreach (var recorrencia in recorrenciasAtivas)
            {
                // ProximaDataGeracao só devolve datas estritamente posteriores à referência — por
                // isso a checagem "hoje é dia de gerar?" usa ontem como referência (ver
                // Recorrencia.ProximaDataGeracao).
                var proximaData = recorrencia.ProximaDataGeracao(dataReferencia.Date.AddDays(-1));
                if (proximaData.Date != dataReferencia.Date)
                    continue;

                // ProximaDataGeracao não olha DataInicio — sem isso, uma recorrência cujo início
                // ainda não chegou poderia coincidir com o dia do mês e gerar cedo demais.
                if (dataReferencia.Date < recorrencia.DataInicio.Date)
                    continue;

                try
                {
                    recorrencia.GerarOcorrencia(dataReferencia);
                }
                catch (DomainException ex)
                {
                    if (!recorrencia.Ativa)
                        await _recorrenciaRepository.AtualizarSalvarAsync(recorrencia, ct);

                    _logger.LogWarning(
                        "Recorrência {IdRecorrencia}: geração não realizada — {Motivo}", recorrencia.IdRecorrencia, ex.Message);
                    continue;
                }

                var gerada = recorrencia.IdConta is not null
                    ? await GerarTransacaoAsync(recorrencia, dataReferencia, ct)
                    : await GerarCompraAsync(recorrencia, dataReferencia, ct);

                if (!gerada)
                    _logger.LogWarning(
                        "Recorrência {IdRecorrencia}: geração bloqueada, revisão do usuário necessária", recorrencia.IdRecorrencia);
            }
        }

        // Mesmas checagens de UC03/UC04 (TransacaoService), usando Recorrencia.IdUsuario no lugar
        // do usuário autenticado da requisição.
        private async Task<bool> GerarTransacaoAsync(Recorrencia recorrencia, DateTime dataReferencia, CancellationToken ct)
        {
            var conta = await _contaRepository.ObterPrimeiroAsync(
                c => c.IdConta == recorrencia.IdConta!.Value && c.IdUsuario == recorrencia.IdUsuario, ct: ct);
            if (conta is null)
            {
                _logger.LogWarning(
                    "Recorrência {IdRecorrencia}: conta {IdConta} não encontrada", recorrencia.IdRecorrencia, recorrencia.IdConta);
                return false;
            }

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == recorrencia.IdCategoria && c.IdUsuario == recorrencia.IdUsuario, ct: ct);
            if (categoria is null)
            {
                _logger.LogWarning(
                    "Recorrência {IdRecorrencia}: categoria {IdCategoria} não encontrada", recorrencia.IdRecorrencia, recorrencia.IdCategoria);
                return false;
            }

            var descricao = $"Recorrência: {categoria.Nome}";

            if (recorrencia.TipoTransacao == TipoTransacao.Despesa)
            {
                // UC05 — mesma regra de UC04: Corrente sempre permite negativar, as demais não.
                if (!conta.PermiteSaldoNegativo())
                {
                    var movimentacao = await _transacaoRepository.ObterSaldoAsync(conta.IdConta, dataReferencia.Date, ct);
                    var saldoResultante = conta.SaldoInicial + movimentacao - recorrencia.Valor;
                    if (saldoResultante < 0)
                    {
                        _logger.LogWarning(
                            "Recorrência {IdRecorrencia}: saldo insuficiente na conta {IdConta}",
                            recorrencia.IdRecorrencia, conta.IdConta);
                        return false;
                    }
                }

                var despesa = Transacao.RegistrarDespesa(
                    recorrencia.IdUsuario, conta.IdConta, categoria.IdCategoria, dataReferencia, recorrencia.Valor,
                    descricao, recorrencia.IdRecorrencia);
                await _transacaoRepository.InserirSalvarAsync(despesa, ct);
                return true;
            }

            var receita = Transacao.RegistrarReceita(
                recorrencia.IdUsuario, conta.IdConta, categoria.IdCategoria, dataReferencia, recorrencia.Valor,
                descricao, recorrencia.IdRecorrencia);
            await _transacaoRepository.InserirSalvarAsync(receita, ct);
            return true;
        }

        // Mesmas checagens de UC14 (CompraService), sempre à vista (numeroParcelas: 1) — uma
        // recorrência gera uma cobrança por ciclo, não um parcelamento.
        private async Task<bool> GerarCompraAsync(Recorrencia recorrencia, DateTime dataReferencia, CancellationToken ct)
        {
            var cartao = await _cartaoRepository.ObterPrimeiroAsync(
                c => c.IdCartao == recorrencia.IdCartao!.Value && c.IdUsuario == recorrencia.IdUsuario,
                incluir: q => q.Include(c => c.Faturas), rastreado: true, ct: ct);
            if (cartao is null || !cartao.Ativo)
            {
                _logger.LogWarning(
                    "Recorrência {IdRecorrencia}: cartão {IdCartao} não encontrado ou inativo",
                    recorrencia.IdRecorrencia, recorrencia.IdCartao);
                return false;
            }

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == recorrencia.IdCategoria && c.IdUsuario == recorrencia.IdUsuario, ct: ct);
            if (categoria is null || categoria.Tipo != TipoCategoria.Despesa || !categoria.Ativa)
            {
                _logger.LogWarning(
                    "Recorrência {IdRecorrencia}: categoria {IdCategoria} inválida para compra",
                    recorrencia.IdRecorrencia, recorrencia.IdCategoria);
                return false;
            }

            // UC15 — soma TODAS as parcelas futuras não pagas do cartão, não só a fatura aberta.
            var totalComprometido = await _parcelaRepository.ObterTotalParcelasNaoPagasAsync(cartao.IdCartao, ct);
            var limiteDisponivel = cartao.LimiteDisponivel(totalComprometido);

            if (recorrencia.Valor > limiteDisponivel)
            {
                _logger.LogWarning(
                    "Recorrência {IdRecorrencia}: limite disponível insuficiente no cartão {IdCartao}",
                    recorrencia.IdRecorrencia, cartao.IdCartao);
                return false;
            }

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var mesReferencia = cartao.CalcularMesReferencia(dataReferencia);
                // Mesmo motivo de CompraService.RegistrarAsync: a fatura precisa de Id real antes
                // de Compra.Registrar montar a Parcela (Parcela.IdFatura não tem navegação EF).
                cartao.ObterOuAbrirFaturaParaLancamento(mesReferencia);
                await _cartaoRepository.SalvarAsync(ct);

                var descricao = $"Recorrência: {categoria.Nome}";
                var novaCompra = Compra.Registrar(
                    recorrencia.IdUsuario, cartao, categoria.IdCategoria, dataReferencia, descricao,
                    recorrencia.Valor, numeroParcelas: 1, limiteDisponivel, recorrencia.IdRecorrencia);

                var parcela = novaCompra.Parcelas.Single();
                var fatura = cartao.ObterOuAbrirFaturaParaLancamento(mesReferencia);
                fatura.RecalcularValorTotal(fatura.ValorTotal + parcela.Valor);

                await _parcelaRepository.InserirAsync(parcela, ct);
                await _compraRepository.InserirAsync(novaCompra, ct);

                return novaCompra;
            }, ct);

            return true;
        }
    }
}
