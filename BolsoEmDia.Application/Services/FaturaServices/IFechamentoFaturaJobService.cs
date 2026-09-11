namespace BolsoEmDia.Application.Services.FaturaServices
{
    public interface IFechamentoFaturaJobService
    {
        /// <summary>
        /// UC20 — Fechar fatura do ciclo. Para cada Cartao ativo cujo DiaFechamento é
        /// <paramref name="dataReferencia"/>, fecha a fatura Aberta do ciclo atual (se houver) e
        /// garante que a fatura do próximo ciclo já exista Aberta.
        /// </summary>
        Task FecharFaturasDoDiaAsync(DateTime dataReferencia, CancellationToken ct = default);
    }
}
