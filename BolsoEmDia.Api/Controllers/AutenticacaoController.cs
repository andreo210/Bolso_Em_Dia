using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.AutenticacaoServices;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AutenticacaoController : MainController
    {
        private readonly IAutenticacaoService _service;

        public AutenticacaoController(IAutenticacaoService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar([FromBody] RegistrarUsuarioDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.LoginAsync(dto, ct);
            return CustomResponse(resultado);
        }
    }
}
