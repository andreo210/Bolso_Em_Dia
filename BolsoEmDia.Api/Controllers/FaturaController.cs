using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.FaturaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Faturas de cartão de crédito do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/faturas")]
    public class FaturaController : MainController
    {
        private readonly IFaturaService _service;

        public FaturaController(IFaturaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as faturas do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma fatura pelo id.
        /// </summary>
        /// <response code="404">Fatura não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Fatura {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Registra o pagamento de uma fatura (inclui verificação de saldo da conta de pagamento).
        /// </summary>
        /// <response code="200">Fatura paga.</response>
        /// <response code="400">Dados inválidos, fatura não encontrada, já paga, categoria não encontrada ou saldo insuficiente.</response>
        // UC17 — Pagar fatura (inclui UC04)
        [HttpPost("{id:int}/pagamento")]
        public async Task<ActionResult> RegistrarPagamento(int id, [FromBody] PagarFaturaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarPagamentoAsync(id, dto, ct);
            return CustomResponse(resultado, HttpStatusCode.OK);
        }
    }
}
