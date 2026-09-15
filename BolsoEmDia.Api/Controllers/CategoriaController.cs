using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CategoriaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Categorias de receita/despesa do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/categorias")]
    public class CategoriaController : MainController
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Lista todas as categorias do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObterTodos(CancellationToken ct)
            => CustomResponse(await _service.ObterTodosAsync(ct));

        /// <summary>
        /// Obtém uma categoria pelo id.
        /// </summary>
        /// <response code="404">Categoria não encontrada.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var resultado = await _service.ObterPorIdAsync(id, ct);
            if (resultado is null) return NotFound($"Categoria {id} não encontrada.");
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Cadastra uma categoria nova.
        /// </summary>
        /// <response code="201">Categoria criada.</response>
        /// <response code="400">Dados inválidos.</response>
        // UC07 — Cadastrar categoria
        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] CriarCategoriaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.CriarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Atualiza o nome de uma categoria existente.
        /// </summary>
        /// <response code="204">Atualizada com sucesso.</response>
        /// <response code="400">Dados inválidos ou categoria não encontrada.</response>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Atualizar(int id, [FromBody] AtualizarCategoriaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var sucesso = await _service.AtualizarAsync(id, dto, ct);
            if (!sucesso) return CustomResponse();

            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Inativa a categoria (soft delete — o histórico de transações é preservado).
        /// </summary>
        /// <response code="204">Inativada com sucesso.</response>
        /// <response code="400">Categoria não encontrada.</response>
        [HttpPatch("{id:int}/inativar")]
        public async Task<ActionResult> Inativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.InativarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Reativa uma categoria previamente inativada.
        /// </summary>
        /// <response code="204">Ativada com sucesso.</response>
        /// <response code="400">Categoria não encontrada.</response>
        [HttpPatch("{id:int}/ativar")]
        public async Task<ActionResult> Ativar(int id, CancellationToken ct)
        {
            var sucesso = await _service.AtivarAsync(id, ct);
            if (!sucesso) return CustomResponse();
            return CustomResponse(null, HttpStatusCode.NoContent);
        }
    }
}
