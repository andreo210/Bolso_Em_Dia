namespace BolsoEmDia.Application.Services.RecorrenciaServices
{
    public interface IRecorrenciaJobService
    {
        /// <summary>
        /// UC21 — Gerar ocorrência de recorrência. Para cada Recorrencia ativa cuja próxima data de
        /// geração é <paramref name="dataReferencia"/>, gera a transação (receita/despesa) ou a
        /// compra correspondente. Nunca lança por bloqueio de saldo/limite — registra e segue para a
        /// próxima recorrência do lote (ver A1 da especificação de UC21).
        /// </summary>
        Task ProcessarGeracoesAsync(DateTime dataReferencia, CancellationToken ct = default);
    }
}
