using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.OrcamentoServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Orçamento mensal por categoria de despesa do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/orcamentos")]
    public class OrcamentoController : MainController
    {
        private readonly IOrcamentoService _service;

        public OrcamentoController(IOrcamentoService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todos os orçamentos do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém um orçamento pelo id.
        /// </summary>
        /// <response code="404">Orçamento não encontrado.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Orçamento {id} não encontrado.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Consulta o progresso do orçamento de uma categoria num mês (percentual consumido e se estourou).
        /// </summary>
        /// <response code="404">Nenhum orçamento definido para essa categoria/mês.</response>
        // UC09 — Acompanhar progresso do orçamento
        [HttpGet("progresso")]
        public async Task<ActionResult> ObterProgresso([FromQuery] int idCategoria, [FromQuery] DateOnly mesReferencia, CancellationToken ct)
        {
            var resultado = await _service.ObterProgressoAsync(idCategoria, mesReferencia, ct);
            if (resultado is null) return NotFound("Nenhum orçamento definido para essa categoria/mês.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Define o orçamento mensal de uma categoria de despesa (ou altera a meta, se já existir).
        /// </summary>
        /// <response code="201">Orçamento definido.</response>
        /// <response code="400">Dados inválidos ou categoria de receita.</response>
        // UC08 — Definir orçamento mensal
        [HttpPost]
        public async Task<ActionResult> Definir([FromBody] DefinirOrcamentoDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.DefinirAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Atualiza o valor da meta de um orçamento existente.
        /// </summary>
        /// <response code="204">Atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos ou orçamento não encontrado.</response>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, [FromBody] AtualizarOrcamentoDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var sucesso = await _service.AtualizarAsync(id, dto, ct);
            if (!sucesso) return CustomResponse();

            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Inativa o orçamento (soft delete — deixa de gerar progresso/alerta de estouro).
        /// </summary>
        /// <response code="204">Inativado com sucesso.</response>
        /// <response code="400">Orçamento não encontrado.</response>
        [HttpPatch("{id:int}/inativar")]
        public async Task<ActionResult> Inativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.InativarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Reativa um orçamento previamente inativado.
        /// </summary>
        /// <response code="204">Ativado com sucesso.</response>
        /// <response code="400">Orçamento não encontrado.</response>
        [HttpPatch("{id:int}/ativar")]
        public async Task<ActionResult> Ativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.AtivarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }
    }
}
