using BolsoEmDia.Front.Models.Request.Orcamento;
using BolsoEmDia.Front.Models.Response.Orcamento;

namespace BolsoEmDia.Front.Services.Servicos.Orcamento
{
    public interface IOrcamentoService
    {
        Task<List<OrcamentoResponse>?> ObterTodos(CancellationToken ct = default);
        Task<OrcamentoResponse?> ObterPorId(int id, CancellationToken ct = default);

        // UC09 — Acompanhar progresso do orçamento. Devolve null quando não há orçamento
        // definido para a categoria/mês (a Api responde 404 nesse caso, não é erro de transporte).
        Task<ProgressoOrcamentoResponse?> ObterProgresso(int idCategoria, DateOnly mesReferencia, CancellationToken ct = default);

        // UC08 — Definir orçamento mensal (upsert: a Api altera a meta se já existir orçamento da categoria naquele mês)
        Task<OrcamentoResponse?> Definir(DefinirOrcamentoRequest request, CancellationToken ct = default);

        Task<bool> Atualizar(int id, AtualizarOrcamentoRequest request, CancellationToken ct = default);
        Task<bool> Ativar(int id, CancellationToken ct = default);
        Task<bool> Inativar(int id, CancellationToken ct = default);
    }
}
