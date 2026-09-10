using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.OrcamentoServices
{
    public class OrcamentoService : IOrcamentoService
    {
        private readonly IOrcamentoRepository _orcamentoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public OrcamentoService(
            IOrcamentoRepository orcamentoRepository,
            ICategoriaRepository categoriaRepository,
            ITransacaoRepository transacaoRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _orcamentoRepository = orcamentoRepository;
            _categoriaRepository = categoriaRepository;
            _transacaoRepository = transacaoRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<OrcamentoDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var orcamento = await ObterOrcamentoDoUsuarioAsync(id, ct);
            return orcamento?.ToDto();
        }

        public async Task<IReadOnlyList<OrcamentoDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var orcamentos = await _orcamentoRepository.ObterAsync(o => o.IdUsuario == IdUsuarioAtual, ct: ct);
            return orcamentos.ToDtoList();
        }

        // UC08 — Definir orçamento mensal (upsert: altera a meta se já existir orçamento da categoria naquele mês)
        public async Task<OrcamentoDto?> DefinirAsync(DefinirOrcamentoDto dto, CancellationToken ct = default)
        {
            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == dto.IdCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (categoria is null)
            {
                _notificador.Add("Categoria não encontrada");
                return null;
            }

            // E1 — categoria de receita não tem orçamento.
            if (categoria.Tipo != TipoCategoria.Despesa)
            {
                _notificador.Add("Categoria de receita não tem orçamento");
                return null;
            }

            var mesReferencia = new DateOnly(dto.MesReferencia.Year, dto.MesReferencia.Month, 1);
            var existente = await _orcamentoRepository.ObterPrimeiroAsync(
                o => o.IdCategoria == dto.IdCategoria && o.MesReferencia == mesReferencia && o.IdUsuario == IdUsuarioAtual,
                rastreado: true, ct: ct);

            if (existente is not null)
            {
                existente.AlterarMeta(dto.ValorMeta);
                await _orcamentoRepository.AtualizarSalvarAsync(existente, ct);
                return existente.ToDto();
            }

            var orcamento = Orcamento.Definir(IdUsuarioAtual, dto.IdCategoria, mesReferencia, dto.ValorMeta);
            var salvo = await _orcamentoRepository.InserirSalvarAsync(orcamento, ct);
            return salvo.ToDto();
        }

        // UC09 — Acompanhar progresso do orçamento
        public async Task<ProgressoOrcamentoDto?> ObterProgressoAsync(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default)
        {
            var mes = new DateOnly(mesReferencia.Year, mesReferencia.Month, 1);
            var orcamento = await _orcamentoRepository.ObterPrimeiroAsync(
                o => o.IdCategoria == idCategoria && o.MesReferencia == mes && o.IdUsuario == IdUsuarioAtual, ct: ct);

            // sem orçamento definido não há progresso a calcular — ausência, nunca 0%/100%+ (ver regras-negocio-financas).
            if (orcamento is null) return null;

            var totalGasto = await CalcularTotalGastoNoMesAsync(idCategoria, mes, ct);

            return new ProgressoOrcamentoDto
            {
                IdCategoria = idCategoria,
                MesReferencia = mes,
                ValorMeta = orcamento.ValorMeta,
                TotalGasto = totalGasto,
                PercentualConsumido = orcamento.PercentualConsumido(totalGasto),
                Estourado = orcamento.Estourado(totalGasto)
            };
        }

        // UC09/UC10 — soma despesas da categoria e das subcategorias dela, com Data <= hoje, dentro do mês de referência.
        private async Task<decimal> CalcularTotalGastoNoMesAsync(int idCategoria, DateOnly mesReferencia, CancellationToken ct)
        {
            var subcategorias = await _categoriaRepository.ObterAsync(
                c => c.IdCategoriaPai == idCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            var idsCategorias = subcategorias.Select(c => c.IdCategoria).Append(idCategoria).ToList();

            var inicioMes = mesReferencia.ToDateTime(TimeOnly.MinValue);
            var fimMes = inicioMes.AddMonths(1).AddTicks(-1);
            var hoje = DateTime.UtcNow.Date;
            var limite = fimMes < hoje ? fimMes : hoje;

            return await _transacaoRepository.ObterTotalDespesasNoPeriodoAsync(idsCategorias, inicioMes, limite, ct);
        }

        // Escopa toda leitura pelo usuário logado: orçamento de outro usuário nunca aparece, nem para confirmar que existe.
        private Task<Orcamento?> ObterOrcamentoDoUsuarioAsync(int id, CancellationToken ct)
            => _orcamentoRepository.ObterPrimeiroAsync(o => o.IdOrcamento == id && o.IdUsuario == IdUsuarioAtual, ct: ct);
    }
}
