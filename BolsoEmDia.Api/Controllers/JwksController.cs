using BolsoEmDia.Application.Services.AutenticacaoServices;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Publica a chave pública usada para validar os JWTs emitidos por /api/v1/auth.
    /// A chave privada nunca passa por aqui — ver RsaKeyService.
    /// </summary>
    [ApiController]
    [Route(".well-known")]
    public class JwksController : ControllerBase
    {
        private readonly RsaKeyService _rsaKeyService;

        public JwksController(RsaKeyService rsaKeyService)
        {
            _rsaKeyService = rsaKeyService;
        }

        [HttpGet("jwks.json")]
        public IActionResult Get()
            => Ok(new { keys = new[] { _rsaKeyService.GetPublicJwk() } });
    }
}
