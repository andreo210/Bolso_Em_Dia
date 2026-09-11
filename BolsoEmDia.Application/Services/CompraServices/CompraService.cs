using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;
using Microsoft.EntityFrameworkCore;

namespace BolsoEmDia.Application.Services.CompraServices
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IParcelaRepository _parcelaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public CompraService(
            ICompraRepository compraRepository,
            ICartaoRepository cartaoRepository,
            ICategoriaRepository categoriaRepository,
            IParcelaRepository parcelaRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _compraRepository = compraRepository;
            _cartaoRepository = cartaoRepository;
            _categoriaRepository = categoriaRepository;
            _parcelaRepository = parcelaRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<CompraDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var compra = await _compraRepository.ObterPrimeiroAsync(
                c => c.IdCompra == id && c.IdUsuario == IdUsuarioAtual,
                incluir: q => q.Include(c => c.Parcelas), ct: ct);
            return compra?.ToDto();
        }

        public async Task<IReadOnlyList<CompraDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var compras = await _compraRepository.ObterAsync(
                filtro: c => c.IdUsuario == IdUsuarioAtual,
                incluir: q => q.Include(c => c.Parcelas),
                ct: ct);

            return compras.ToDtoList();
        }

        // UC14 — Registrar compra no cartão (inclui UC15 — verificar limite, UC16 — gerar parcelas)
        public async Task<CompraDto?> RegistrarAsync(CriarCompraDto dto, CancellationToken ct = default)
        {
            var cartao = await _cartaoRepository.ObterPrimeiroAsync(
                c => c.IdCartao == dto.IdCartao && c.IdUsuario == IdUsuarioAtual,
                incluir: q => q.Include(c => c.Faturas), rastreado: true, ct: ct);
            if (cartao is null)
                _notificador.Add("Cartão não encontrado");
            else if (!cartao.Ativo)
                _notificador.Add("Cartão inativo");

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

            // UC15 — Verificar limite disponível do cartão: soma TODAS as parcelas futuras não
            // pagas (não só a fatura aberta) — é isso que Cartao.LimiteDisponivel espera.
            var totalComprometido = await _parcelaRepository.ObterTotalParcelasNaoPagasAsync(cartao!.IdCartao, ct);
            var limiteDisponivel = cartao.LimiteDisponivel(totalComprometido);

            // E1 — bloqueia a compra inteira (à vista ou parcelada) antes de tocar o domínio,
            // mesmo padrão do resto do app: notificador acumula, exceção fica só para invariante.
            if (dto.ValorTotal > limiteDisponivel)
            {
                _notificador.Add("Limite disponível insuficiente");
                return null;
            }

            var compra = await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                // Parcela.IdFatura é uma referência entre agregados por id, sem navegação EF —
                // por isso as faturas dos N ciclos precisam já ter Id real ANTES de Compra.Registrar
                // montar as parcelas (senão uma fatura aberta agora grava IdFatura=0, não o Id de
                // verdade). Resolve/abre todas aqui e salva já, depois é só reaproveitar.
                var mesReferenciaBase = cartao.CalcularMesReferencia(dto.Data);
                for (var numero = 0; numero < dto.NumeroParcelas; numero++)
                    cartao.ObterOuAbrirFaturaParaLancamento(mesReferenciaBase.AddMonths(numero));

                await _cartaoRepository.SalvarAsync(ct);

                // UC16 — Cartao.ObterOuAbrirFaturaParaLancamento (chamado de dentro de
                // Compra.Registrar) é idempotente: como as faturas já existem acima, ele só
                // reaproveita — nenhuma fatura nova é aberta aqui, e o Id gravado é o real.
                var novaCompra = Compra.Registrar(
                    IdUsuarioAtual, cartao, dto.IdCategoria, dto.Data, dto.Descricao,
                    dto.ValorTotal, dto.NumeroParcelas, limiteDisponivel);

                // Fatura.ValorTotal é denormalizado: soma-se a cada escrita de Parcela nela.
                // Anda por referência de objeto (não por Id) porque, nos testes com repositório
                // fake, faturas novas nunca ganham Id real — casar pelo Id ficaria ambíguo.
                // Cada parcela é gravada explicitamente (em vez de confiar só na cascata via
                // Compra.Parcelas) para que ObterTotalParcelasNaoPagasAsync já enxergue as
                // parcelas desta compra numa próxima chamada, sem depender de comportamento de
                // cascata específico de EF que o repositório fake não simula.
                foreach (var parcela in novaCompra.Parcelas)
                {
                    var fatura = cartao.ObterOuAbrirFaturaParaLancamento(mesReferenciaBase.AddMonths(parcela.Numero - 1));
                    fatura.RecalcularValorTotal(fatura.ValorTotal + parcela.Valor);
                    await _parcelaRepository.InserirAsync(parcela, ct);
                }

                await _compraRepository.InserirAsync(novaCompra, ct);

                return novaCompra;
            }, ct);

            return compra.ToDto();
        }
    }
}
