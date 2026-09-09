using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Models.Mappers;
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
        private readonly ICurrentUser _currentUser;
        private readonly INotificadorService _notificador;

        public TransacaoService(
            ITransacaoRepository transacaoRepository,
            IContaRepository contaRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUser currentUser,
            INotificadorService notificador)
        {
            _transacaoRepository = transacaoRepository;
            _contaRepository = contaRepository;
            _categoriaRepository = categoriaRepository;
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
    }
}
