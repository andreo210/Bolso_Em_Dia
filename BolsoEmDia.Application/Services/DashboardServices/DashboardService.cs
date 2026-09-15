using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.IRepositorio;
using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Application.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private const int MesesEvolucao = 6;

        private readonly ITransacaoRepository _transacaoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICurrentUser _currentUser;

        public DashboardService(
            ITransacaoRepository transacaoRepository,
            ICategoriaRepository categoriaRepository,
            ICurrentUser currentUser)
        {
            _transacaoRepository = transacaoRepository;
            _categoriaRepository = categoriaRepository;
            _currentUser = currentUser;
        }

        private string IdUsuarioAtual => _currentUser.UserId!;

        // UC22 — Ver dashboard
        public async Task<DashboardDto> ObterAsync(CancellationToken ct = default)
        {
            var hoje = DateTime.UtcNow.Date;
            var mesAtual = new DateOnly(hoje.Year, hoje.Month, 1);
            var inicioEvolucao = mesAtual.AddMonths(-(MesesEvolucao - 1));

            var inicioEvolucaoDt = inicioEvolucao.ToDateTime(TimeOnly.MinValue);
            var fimMesAtualDt = mesAtual.AddMonths(1).ToDateTime(TimeOnly.MinValue).AddTicks(-1);

            var evolucaoBruta = await _transacaoRepository.ObterEvolucaoMensalAsync(
                IdUsuarioAtual, inicioEvolucaoDt, fimMesAtualDt, ct);

            // Preenche todos os meses do intervalo (mesmo sem transação) para o gráfico não ter buraco.
            var evolucaoMensal = new List<EvolucaoMensalDto>();
            for (var mes = inicioEvolucao; mes <= mesAtual; mes = mes.AddMonths(1))
            {
                var doMes = evolucaoBruta.FirstOrDefault(e => e.Ano == mes.Year && e.Mes == mes.Month);
                evolucaoMensal.Add(new EvolucaoMensalDto
                {
                    Mes = mes,
                    TotalReceitas = doMes.TotalReceitas,
                    TotalDespesas = doMes.TotalDespesas
                });
            }

            var resumoMesAtual = evolucaoMensal.Last();

            var gastosPorCategoria = await ObterGastosPorCategoriaAsync(mesAtual, resumoMesAtual.TotalDespesas, ct);

            return new DashboardDto
            {
                MesReferencia = mesAtual,
                TotalReceitasMes = resumoMesAtual.TotalReceitas,
                TotalDespesasMes = resumoMesAtual.TotalDespesas,
                SaldoMes = resumoMesAtual.TotalReceitas - resumoMesAtual.TotalDespesas,
                GastosPorCategoria = gastosPorCategoria,
                EvolucaoMensal = evolucaoMensal
            };
        }

        private async Task<List<GastoCategoriaDto>> ObterGastosPorCategoriaAsync(
            DateOnly mesAtual, decimal totalDespesasMes, CancellationToken ct)
        {
            var inicioMesDt = mesAtual.ToDateTime(TimeOnly.MinValue);
            var fimMesDt = mesAtual.AddMonths(1).ToDateTime(TimeOnly.MinValue).AddTicks(-1);

            var gastosBrutos = await _transacaoRepository.ObterGastosPorCategoriaAsync(
                IdUsuarioAtual, inicioMesDt, fimMesDt, ct);

            if (gastosBrutos.Count == 0) return new List<GastoCategoriaDto>();

            var idsCategoria = gastosBrutos.Select(g => g.IdCategoria).ToList();
            var categorias = await _categoriaRepository.ObterAsync(
                c => idsCategoria.Contains(c.IdCategoria) && c.IdUsuario == IdUsuarioAtual, ct: ct);
            var nomesPorId = categorias.ToDictionary(c => c.IdCategoria, c => c.Nome);

            return gastosBrutos
                .Select(g => new GastoCategoriaDto
                {
                    IdCategoria = g.IdCategoria,
                    NomeCategoria = nomesPorId.GetValueOrDefault(g.IdCategoria, "—"),
                    Total = g.Total,
                    Percentual = totalDespesasMes > 0 ? g.Total / totalDespesasMes : 0
                })
                .OrderByDescending(g => g.Total)
                .ToList();
        }
    }
}
