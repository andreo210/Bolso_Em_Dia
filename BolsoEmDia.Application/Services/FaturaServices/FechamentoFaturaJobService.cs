using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Application.Services.FaturaServices
{
    /// <summary>
    /// UC20 — Fechar fatura do ciclo. Roda dentro do escopo aberto pelo FechamentoFaturaJob
    /// (hosted service, em BolsoEmDia.Api).
    /// </summary>
    public class FechamentoFaturaJobService : IFechamentoFaturaJobService
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IFaturaRepository _faturaRepository;

        public FechamentoFaturaJobService(ICartaoRepository cartaoRepository, IFaturaRepository faturaRepository)
        {
            _cartaoRepository = cartaoRepository;
            _faturaRepository = faturaRepository;
        }

        public async Task FecharFaturasDoDiaAsync(DateTime dataReferencia, CancellationToken ct = default)
        {
            var cartoesDoDia = await _cartaoRepository.ObterAsync(
                filtro: c => c.Ativo && c.DiaFechamento == dataReferencia.Day, ct: ct);

            foreach (var cartao in cartoesDoDia)
            {
                var faturaAtual = await _faturaRepository.ObterPrimeiroAsync(
                    f => f.IdCartao == cartao.IdCartao && f.Status == StatusFatura.Aberta, rastreado: true, ct: ct);

                if (faturaAtual is not null)
                {
                    faturaAtual.Fechar();
                    await _faturaRepository.AtualizarSalvarAsync(faturaAtual, ct);
                }

                // Sem fatura aberta ainda (cartão sem nenhuma compra até hoje) — usa o ciclo atual
                // (hoje cai justo no dia de fechamento) como referência do "próximo ciclo" a garantir,
                // igual ao caso em que já havia uma fatura para fechar (ver especificacao-casos-de-uso.md).
                var mesReferenciaAtual = faturaAtual?.MesReferencia ?? cartao.CalcularMesReferencia(dataReferencia);
                var proximoMes = mesReferenciaAtual.AddMonths(1);

                var existeProxima = await _faturaRepository.ExisteAsync(
                    f => f.IdCartao == cartao.IdCartao && f.MesReferencia == proximoMes, ct);

                if (!existeProxima)
                {
                    var novaFatura = cartao.AbrirFatura(
                        proximoMes, cartao.CalcularDataFechamento(proximoMes), cartao.CalcularDataVencimento(proximoMes));
                    await _faturaRepository.InserirSalvarAsync(novaFatura, ct);
                }
            }
        }
    }
}
