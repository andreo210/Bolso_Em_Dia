using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.ContaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// CRUD de contas (corrente, poupança, carteira, investimento) do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/contas")]
    public class ContaController : MainController
    {
        private readonly IContaService _service;

        public ContaController(IContaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as contas do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma conta pelo id.
        /// </summary>
        /// <response code="404">Conta não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Conta {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Consulta o saldo atual da conta.
        /// </summary>
        /// <response code="404">Conta não encontrada.</response>
        // UC05 — Verificar saldo da conta
        [HttpGet("{id:int}/saldo")]
        public async Task<ActionResult> ObterSaldo(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterSaldoAsync(id, ct);
            if (resultado is null) return NotFound($"Conta {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Cadastra uma conta nova.
        /// </summary>
        /// <response code="201">Conta criada.</response>
        /// <response code="400">Dados inválidos.</response>
        // UC01 — Cadastrar conta
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarContaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Atualiza o nome de uma conta existente.
        /// </summary>
        /// <response code="204">Atualizada com sucesso.</response>
        /// <response code="400">Dados inválidos ou conta não encontrada.</response>
        // UC02 — Editar conta
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, [FromBody] AtualizarContaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var sucesso = await _service.AtualizarAsync(id, dto, ct);
            if (!sucesso) return CustomResponse();

            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Inativa a conta (soft delete — a conta deixa de aparecer nas operações do dia a dia).
        /// </summary>
        /// <response code="204">Inativada com sucesso.</response>
        /// <response code="400">Conta não encontrada.</response>
        // UC02 — Inativar conta
        [HttpPatch("{id:int}/inativar")]
        public async Task<ActionResult> Inativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.InativarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Reativa uma conta previamente inativada.
        /// </summary>
        /// <response code="204">Ativada com sucesso.</response>
        /// <response code="400">Conta não encontrada.</response>
        [HttpPatch("{id:int}/ativar")]
        public async Task<ActionResult> Ativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.AtivarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }
    }
}
