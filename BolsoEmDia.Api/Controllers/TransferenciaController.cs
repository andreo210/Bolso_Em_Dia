using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.TransferenciaServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Transferências entre contas do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/transferencias")]
    public class TransferenciaController : MainController
    {
        private readonly ITransferenciaService _service;

        public TransferenciaController(ITransferenciaService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Registra uma transferência entre duas contas do usuário: debita o valor da conta de
        /// origem e credita o mesmo valor na conta de destino, de forma atômica.
        /// </summary>
        /// <response code="201">Transferência registrada.</response>
        /// <response code="400">Dados inválidos, conta de origem/destino inexistente, mesma conta em ambos os lados ou saldo insuficiente na origem.</response>
        // UC06 — Transferir entre contas (inclui UC05)
        [HttpPost]
        public async Task<ActionResult> Registrar([FromBody] CriarTransferenciaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
