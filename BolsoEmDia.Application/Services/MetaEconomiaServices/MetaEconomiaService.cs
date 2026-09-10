using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;
using Microsoft.EntityFrameworkCore;

namespace BolsoEmDia.Application.Services.MetaEconomiaServices
{
    public class MetaEconomiaService : IMetaEconomiaService
    {
        private readonly IMetaEconomiaRepository _metaRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public MetaEconomiaService(
            IMetaEconomiaRepository metaRepository,
            IContaRepository contaRepository,
            ITransacaoRepository transacaoRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _metaRepository = metaRepository;
            _contaRepository = contaRepository;
            _transacaoRepository = transacaoRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<MetaEconomiaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var meta = await ObterMetaDoUsuarioAsync(id, rastreado: false, ct, incluirAportes: true);
            return meta?.ToDto();
        }

        public async Task<IReadOnlyList<MetaEconomiaDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var metas = await _metaRepository.ObterAsync(
                filtro: m => m.IdUsuario == IdUsuarioAtual,
                incluir: q => q.Include(m => m.Aportes),
                ct: ct);

            return metas.ToDtoList();
        }

        public async Task<IReadOnlyList<AporteMetaDto>> ObterAportesAsync(int idMeta, CancellationToken ct = default)
        {
            var meta = await ObterMetaDoUsuarioAsync(idMeta, rastreado: false, ct, incluirAportes: true);
            return meta?.Aportes.ToDtoList() ?? new List<AporteMetaDto>();
        }

        // UC11 — Criar meta de economia
        public async Task<MetaEconomiaDto?> CriarAsync(CriarMetaEconomiaDto dto, CancellationToken ct = default)
        {
            // E1 — data-alvo no passado.
            if (dto.DataAlvo.HasValue && dto.DataAlvo.Value.Date < DateTime.UtcNow.Date)
            {
                _notificador.Add("Data-alvo não pode estar no passado");
                return null;
            }

            // Pré-condição: se vinculada a uma conta, ela precisa existir e pertencer ao usuário.
            if (dto.IdConta.HasValue)
            {
                var conta = await _contaRepository.ObterPrimeiroAsync(
                    c => c.IdConta == dto.IdConta.Value && c.IdUsuario == IdUsuarioAtual, ct: ct);
                if (conta is null)
                {
                    _notificador.Add("Conta não encontrada");
                    return null;
                }
            }

            var meta = MetaEconomia.Criar(IdUsuarioAtual, dto.Nome, dto.ValorAlvo, dto.DataAlvo, dto.IdConta);
            var salva = await _metaRepository.InserirSalvarAsync(meta, ct);
            return salva.ToDto();
        }

        // UC12 — Registrar aporte em meta
        public async Task<AporteMetaDto?> RegistrarAporteAsync(int idMeta, RegistrarAporteMetaDto dto, CancellationToken ct = default)
        {
            var meta = await ObterMetaDoUsuarioAsync(idMeta, rastreado: true, ct, incluirAportes: true);
            if (meta is null)
            {
                _notificador.Add("Meta não encontrada");
                return null;
            }

            // Se ligado a uma transação, ela precisa existir e pertencer ao usuário — a meta em si
            // não valida saldo, quem já validou foi a transação/transferência de origem.
            if (dto.IdTransacao.HasValue)
            {
                var transacaoExiste = await _transacaoRepository.ExisteAsync(
                    t => t.IdTransacao == dto.IdTransacao.Value && t.IdUsuario == IdUsuarioAtual, ct);
                if (!transacaoExiste)
                {
                    _notificador.Add("Transação não encontrada");
                    return null;
                }
            }

            meta.RegistrarAporte(dto.Valor, dto.Data, dto.IdTransacao);
            await _metaRepository.AtualizarSalvarAsync(meta, ct);

            return meta.Aportes.Last().ToDto();
        }

        // Escopa toda leitura/escrita pelo usuário logado: meta de outro usuário nunca aparece,
        // nem para confirmar que existe.
        private Task<MetaEconomia?> ObterMetaDoUsuarioAsync(int id, bool rastreado, CancellationToken ct, bool incluirAportes = false)
            => _metaRepository.ObterPrimeiroAsync(
                m => m.IdMeta == id && m.IdUsuario == IdUsuarioAtual,
                incluir: incluirAportes ? q => q.Include(m => m.Aportes) : null,
                rastreado: rastreado,
                ct: ct);
    }
}
