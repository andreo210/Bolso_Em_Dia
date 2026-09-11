using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.RecorrenciaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Recorrências (receitas, despesas ou compras no cartão) do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/recorrencias")]
    public class RecorrenciaController : MainController
    {
        private readonly IRecorrenciaService _service;

        public RecorrenciaController(IRecorrenciaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as recorrências do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma recorrência pelo id.
        /// </summary>
        /// <response code="404">Recorrência não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Recorrência {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Cria uma nova recorrência.
        /// </summary>
        /// <response code="201">Recorrência criada.</response>
        /// <response code="400">Dados inválidos, conta/cartão ou categoria não encontrados.</response>
        // UC18 — Criar recorrência
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarRecorrenciaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Pausa a recorrência — impede novas gerações futuras, sem afetar o que já foi gerado.
        /// </summary>
        /// <response code="204">Pausada com sucesso.</response>
        /// <response code="400">Recorrência não encontrada.</response>
        // UC19 — Pausar / cancelar recorrência
        [HttpPatch("{id:int}/pausar")]
        public async Task<ActionResult> Pausar(int id, CancellationToken ct)
        {
            var sucesso = await _service.PausarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Reativa uma recorrência previamente pausada.
        /// </summary>
        /// <response code="204">Reativada com sucesso.</response>
        /// <response code="400">Recorrência não encontrada.</response>
        // UC19 — Pausar / cancelar recorrência (reativar)
        [HttpPatch("{id:int}/reativar")]
        public async Task<ActionResult> Reativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.ReativarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }
    }
}
