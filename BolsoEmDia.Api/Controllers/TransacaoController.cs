using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.TransacaoServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/transacoes")]
    public class TransacaoController : MainController
    {
        private readonly ITransacaoService _service;

        public TransacaoController(ITransacaoService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        // UC03 — Registrar receita
        [HttpPost("receitas")]
        public async Task<ActionResult> RegistrarReceita([FromBody] CriarReceitaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarReceitaAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
