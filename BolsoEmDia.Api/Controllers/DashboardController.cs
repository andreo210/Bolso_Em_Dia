using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Services.DashboardServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolsoEmDia.Api.Controllers
{
    /// <summary>
    /// Resumo consolidado do usuário autenticado: receitas/despesas do mês, gastos por categoria
    /// e evolução mensal — consulta pura sobre Transacao, sem entidade própria.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/dashboard")]
    public class DashboardController : MainController
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service, INotificadorService notificador) : base(notificador)
        {
            _service = service;
        }

        /// <summary>
        /// Obtém o dashboard do usuário autenticado (resumo do mês, gastos por categoria e evolução mensal).
        /// </summary>
        // UC22 — Ver dashboard
        [HttpGet]
        public async Task<ActionResult> Obter(CancellationToken ct)
            => CustomResponse(await _service.ObterAsync(ct));
    }
}
