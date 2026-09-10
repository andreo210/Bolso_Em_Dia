using System.Net;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.TransacaoServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Lançamentos financeiros (receitas e despesas) do usuário autenticado.
    /// </summary>
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

        /// <summary>
        /// Registra uma receita, somando o valor ao saldo da conta informada.
        /// </summary>
        /// <response code="201">Receita registrada.</response>
        /// <response code="400">Dados inválidos ou conta/categoria inexistente.</response>
        // UC03 — Registrar receita
        [HttpPost("receitas")]
        public async Task<ActionResult> RegistrarReceita([FromBody] CriarReceitaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarReceitaAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }

        /// <summary>
        /// Registra uma despesa, subtraindo o valor do saldo da conta informada. Bloqueia se o saldo
        /// resultante ficar negativo em conta que não permite (Poupança/Carteira/Investimento).
        /// </summary>
        /// <response code="201">Despesa registrada. Se a categoria tiver orçamento estourado no mês, o DTO traz <c>AlertaOrcamento</c> preenchido (não é erro).</response>
        /// <response code="400">Dados inválidos, conta/categoria inexistente ou saldo insuficiente.</response>
        // UC04 — Registrar despesa (inclui UC05; estende UC10)
        [HttpPost("despesas")]
        public async Task<ActionResult> RegistrarDespesa([FromBody] CriarDespesaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationResponse(ModelState);

            var resultado = await _service.RegistrarDespesaAsync(dto, ct);
            return CustomResponse(resultado, HttpStatusCode.Created);
        }
    }
}
