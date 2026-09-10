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
    }
}
