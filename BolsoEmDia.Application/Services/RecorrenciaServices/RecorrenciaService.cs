using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.RecorrenciaServices
{
    public class RecorrenciaService : IRecorrenciaService
    {
        private readonly IRecorrenciaRepository _recorrenciaRepository;
        private readonly IContaRepository _contaRepository;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public RecorrenciaService(
            IRecorrenciaRepository recorrenciaRepository,
            IContaRepository contaRepository,
            ICartaoRepository cartaoRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _recorrenciaRepository = recorrenciaRepository;
            _contaRepository = contaRepository;
            _cartaoRepository = cartaoRepository;
            _categoriaRepository = categoriaRepository;
            _currentUser = currentUser;
            _notificador = notificador;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        public async Task<RecorrenciaDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var recorrencia = await _recorrenciaRepository.ObterPrimeiroAsync(
                r => r.IdRecorrencia == id && r.IdUsuario == IdUsuarioAtual, rastreado: false, ct: ct);
            return recorrencia?.ToDto();
        }

        public async Task<IReadOnlyList<RecorrenciaDto>> ObterTodosAsync(CancellationToken ct = default)
        {
            var recorrencias = await _recorrenciaRepository.ObterAsync(filtro: r => r.IdUsuario == IdUsuarioAtual, ct: ct);
            return recorrencias.ToDtoList();
        }

        // UC18 — Criar recorrência
        public async Task<RecorrenciaDto?> CriarAsync(CriarRecorrenciaDto dto, CancellationToken ct = default)
        {
            // E1 — exatamente uma de IdConta/IdCartao, nunca as duas, nunca nenhuma.
            if ((dto.IdConta is null) == (dto.IdCartao is null))
            {
                _notificador.Add("Recorrência deve estar ligada a exatamente uma conta ou um cartão");
                return null;
            }

            if (dto.IdConta is not null)
            {
                var contaExiste = await _contaRepository.ExisteAsync(
                    c => c.IdConta == dto.IdConta && c.IdUsuario == IdUsuarioAtual, ct);
                if (!contaExiste)
                    _notificador.Add("Conta não encontrada");

                if (dto.TipoTransacao is not (TipoTransacao.Receita or TipoTransacao.Despesa))
                    _notificador.Add("Tipo da transação deve ser receita ou despesa");
            }
            else
            {
                var cartaoExiste = await _cartaoRepository.ExisteAsync(
                    c => c.IdCartao == dto.IdCartao && c.IdUsuario == IdUsuarioAtual, ct);
                if (!cartaoExiste)
                    _notificador.Add("Cartão não encontrado");
            }

            var categoria = await _categoriaRepository.ObterPrimeiroAsync(
                c => c.IdCategoria == dto.IdCategoria && c.IdUsuario == IdUsuarioAtual, ct: ct);
            if (categoria is null)
            {
                _notificador.Add("Categoria não encontrada");
            }
            else
            {
                // Recorrência em cartão sempre gera compra (UC14), que só aceita categoria de despesa.
                var tipoCategoriaEsperado = dto.IdCartao is not null ? TipoCategoria.Despesa
                    : dto.TipoTransacao switch
                    {
                        TipoTransacao.Receita => TipoCategoria.Receita,
                        TipoTransacao.Despesa => TipoCategoria.Despesa,
                        _ => (TipoCategoria?)null
                    };

                if (tipoCategoriaEsperado is not null && categoria.Tipo != tipoCategoriaEsperado)
                {
                    var nomeTipo = tipoCategoriaEsperado == TipoCategoria.Receita ? "receita" : "despesa";
                    _notificador.Add($"Categoria informada não é uma categoria de {nomeTipo}");
                }
            }

            if (_notificador.TemNotificacao())
                return null;

            var recorrencia = Recorrencia.Criar(
                IdUsuarioAtual, dto.IdConta, dto.IdCartao, dto.IdCategoria, dto.TipoTransacao,
                dto.Valor, dto.Frequencia, dto.DiaGeracao, dto.DataInicio, dto.DataFim);

            var salva = await _recorrenciaRepository.InserirSalvarAsync(recorrencia, ct);
            return salva.ToDto();
        }

        // UC19 — Pausar / cancelar recorrência
        public async Task<bool> PausarAsync(int id, CancellationToken ct = default)
            => await AlterarSituacaoAsync(id, ativar: false, ct);

        // UC19 — Pausar / cancelar recorrência (reativar)
        public async Task<bool> ReativarAsync(int id, CancellationToken ct = default)
            => await AlterarSituacaoAsync(id, ativar: true, ct);

        private async Task<bool> AlterarSituacaoAsync(int id, bool ativar, CancellationToken ct)
        {
            var recorrencia = await _recorrenciaRepository.ObterPrimeiroAsync(
                r => r.IdRecorrencia == id && r.IdUsuario == IdUsuarioAtual, rastreado: true, ct: ct);
            if (recorrencia is null)
            {
                _notificador.Add("Recorrência não encontrada");
                return false;
            }

            if (ativar) recorrencia.Reativar();
            else recorrencia.Pausar();

            return await _recorrenciaRepository.AtualizarSalvarAsync(recorrencia, ct);
        }
    }
}
