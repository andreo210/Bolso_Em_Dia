using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.MetaEconomiaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Metas de economia e aportes do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/metas")]
    public class MetaEconomiaController : MainController
    {
        private readonly IMetaEconomiaService _service;

        public MetaEconomiaController(IMetaEconomiaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as metas de economia do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma meta de economia pelo id.
        /// </summary>
        /// <response code="404">Meta não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Meta {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Lista os aportes já registrados numa meta.
        /// </summary>
        /// <response code="404">Meta não encontrada.</response>
        [HttpGet("{id:int}/aportes")]
        public async Task<ActionResult> ObterAportes(int id, CancellationToken ct)
        {
            var meta = await _service.ObterPorIdAsync(id, ct);
            if (meta is null) return NotFound($"Meta {id} não encontrada.");
            return CustomResponse(await _service.ObterAportesAsync(id, ct));
        }

        /// <summary>
        /// Cria uma nova meta de economia.
        /// </summary>
        /// <response code="201">Meta criada.</response>
        /// <response code="400">Dados inválidos ou conta não encontrada.</response>
        // UC11 — Criar meta de economia
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarMetaEconomiaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Registra um aporte numa meta existente.
        /// </summary>
        /// <response code="201">Aporte registrado.</response>
        /// <response code="400">Dados inválidos, meta não encontrada ou transação não encontrada.</response>
        // UC12 — Registrar aporte em meta
        [HttpPost("{id:int}/aportes")]
        public async Task<ActionResult> RegistrarAporte(int id, [FromBody] RegistrarAporteMetaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarAporteAsync(id, dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
