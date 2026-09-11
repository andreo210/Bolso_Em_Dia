using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CompraServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Compras no cartão de crédito do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/compras")]
    public class CompraController : MainController
    {
        private readonly ICompraService _service;

        public CompraController(ICompraService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as compras no cartão do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma compra pelo id.
        /// </summary>
        /// <response code="404">Compra não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Compra {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Registra uma compra no cartão (inclui verificação de limite e geração de parcelas).
        /// </summary>
        /// <response code="201">Compra registrada.</response>
        /// <response code="400">Dados inválidos, cartão/categoria não encontrados ou limite disponível insuficiente.</response>
        // UC14 — Registrar compra no cartão (inclui UC15, UC16)
        [HttpPost]
        public async Task<ActionResult> Registrar([FromBody] CriarCompraDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
