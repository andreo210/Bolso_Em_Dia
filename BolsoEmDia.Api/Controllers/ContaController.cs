using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.ContaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
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

        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Conta {id} não encontrada.");
            return CustomResponse(resultado);
        }

        // UC05 — Verificar saldo da conta
        [HttpGet("{id:int}/saldo")]
        public async Task<ActionResult> ObterSaldo(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterSaldoAsync(id, ct);
            if (resultado is null) return NotFound($"Conta {id} não encontrada.");
            return CustomResponse(resultado);
        }

        // UC01 — Cadastrar conta
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarContaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        // UC02 — Editar conta
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, [FromBody] AtualizarContaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var sucesso = await _service.AtualizarAsync(id, dto, ct);
            if (!sucesso) return CustomResponse();

            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        // UC02 — Inativar conta
        [HttpPatch("{id:int}/inativar")]
        public async Task<ActionResult> Inativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.InativarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        [HttpPatch("{id:int}/ativar")]
        public async Task<ActionResult> Ativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.AtivarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }
    }
}
