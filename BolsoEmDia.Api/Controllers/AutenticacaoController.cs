using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.AutenticacaoServices;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Cadastro e autenticação de usuários. Endpoints anônimos — emitem o JWT usado pelas demais rotas.
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AutenticacaoController : MainController
    {
        private readonly IAutenticacaoService _service;

        public AutenticacaoController(IAutenticacaoService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Cadastra um novo usuário.
        /// </summary>
        /// <response code="201">Usuário criado.</response>
        /// <response code="400">Dados inválidos ou e-mail já cadastrado.</response>
        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar([FromBody] RegistrarUsuarioDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Autentica um usuário e devolve o JWT de acesso.
        /// </summary>
        /// <response code="200">Autenticado; retorna o token e sua expiração.</response>
        /// <response code="400">Credenciais inválidas.</response>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.LoginAsync(dto, ct);
            return CustomResponse(resultado);
        }
    }
}
