using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CartaoServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Cartões de crédito do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/cartoes")]
    public class CartaoController : MainController
    {
        private readonly ICartaoService _service;

        public CartaoController(ICartaoService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todos os cartões do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém um cartão pelo id.
        /// </summary>
        /// <response code="404">Cartão não encontrado.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Cartão {id} não encontrado.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Cadastra um novo cartão de crédito.
        /// </summary>
        /// <response code="201">Cartão criado.</response>
        /// <response code="400">Dados inválidos ou conta de pagamento não encontrada.</response>
        // UC13 — Cadastrar cartão de crédito
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarCartaoDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
