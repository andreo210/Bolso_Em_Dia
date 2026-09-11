using BolsoEmDia.Application.Services.FaturaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    // ======================= UC20 — Fechar fatura do ciclo =======================
    public class FechamentoFaturaJobServiceTests
    {
        private static (FechamentoFaturaJobService Service, ArmazemFake Armazem) Montar(ArmazemFake? armazem = null)
        {
            var arm = armazem ?? new ArmazemFake();
            var cartoes = new CartaoRepositoryFake(arm);
            var faturas = new FaturaRepositoryFake(arm);

            var service = new FechamentoFaturaJobService(cartoes, faturas);

            return (service, arm);
        }

        [Fact]
        public async Task FecharFaturasDoDiaAsync_cartao_no_dia_de_fechamento_fecha_fatura_aberta_e_abre_a_do_proximo_ciclo()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(diaFechamento: hoje.Day);
            armazem.Semear(cartao);
            var faturaAtual = Fabrica.Fatura(cartao);
            armazem.Semear(faturaAtual);

            var (service, arm) = Montar(armazem);

            await service.FecharFaturasDoDiaAsync(hoje);

            var faturas = arm.Tabela<Fatura>();
            Assert.Equal(2, faturas.Count);
            Assert.Equal(StatusFatura.Fechada, faturas.Single(f => f.IdFatura == faturaAtual.IdFatura).Status);

            var novaFatura = faturas.Single(f => f.IdFatura != faturaAtual.IdFatura);
            Assert.Equal(StatusFatura.Aberta, novaFatura.Status);
            Assert.Equal(faturaAtual.MesReferencia.AddMonths(1), novaFatura.MesReferencia);
        }

        [Fact]
        public async Task FecharFaturasDoDiaAsync_cartao_fora_do_dia_de_fechamento_nao_altera_faturas()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(diaFechamento: hoje.AddDays(1).Day);
            armazem.Semear(cartao);
            var faturaAtual = Fabrica.Fatura(cartao);
            armazem.Semear(faturaAtual);

            var (service, arm) = Montar(armazem);

            await service.FecharFaturasDoDiaAsync(hoje);

            var faturas = arm.Tabela<Fatura>();
            Assert.Single(faturas);
            Assert.Equal(StatusFatura.Aberta, faturas[0].Status);
        }

        [Fact]
        public async Task FecharFaturasDoDiaAsync_cartao_inativo_nao_e_processado()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(diaFechamento: hoje.Day);
            armazem.Semear(cartao);
            var faturaAtual = Fabrica.Fatura(cartao);
            armazem.Semear(faturaAtual);
            cartao.Desativar();

            var (service, arm) = Montar(armazem);

            await service.FecharFaturasDoDiaAsync(hoje);

            var faturas = arm.Tabela<Fatura>();
            Assert.Single(faturas);
            Assert.Equal(StatusFatura.Aberta, faturas[0].Status);
        }

        // "primeira fatura é aberta pela primeira compra ou pelo job de fechamento" (UC13/UC20):
        // cartão sem nenhuma compra ainda não deve quebrar o job — ele só garante o próximo ciclo.
        [Fact]
        public async Task FecharFaturasDoDiaAsync_cartao_sem_fatura_aberta_apenas_garante_o_proximo_ciclo()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(diaFechamento: hoje.Day);
            armazem.Semear(cartao);

            var (service, arm) = Montar(armazem);

            await service.FecharFaturasDoDiaAsync(hoje);

            var faturas = arm.Tabela<Fatura>();
            var novaFatura = Assert.Single(faturas);
            Assert.Equal(StatusFatura.Aberta, novaFatura.Status);
            Assert.Equal(cartao.CalcularMesReferencia(hoje).AddMonths(1), novaFatura.MesReferencia);
        }

        [Fact]
        public async Task FecharFaturasDoDiaAsync_fatura_do_proximo_ciclo_ja_existente_nao_duplica()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(diaFechamento: hoje.Day);
            armazem.Semear(cartao);
            var faturaAtual = Fabrica.Fatura(cartao);
            armazem.Semear(faturaAtual);

            var proximoMes = faturaAtual.MesReferencia.AddMonths(1);
            var proximaFatura = cartao.AbrirFatura(
                proximoMes, cartao.CalcularDataFechamento(proximoMes), cartao.CalcularDataVencimento(proximoMes));
            armazem.Semear(proximaFatura);

            var (service, arm) = Montar(armazem);

            await service.FecharFaturasDoDiaAsync(hoje);

            var faturas = arm.Tabela<Fatura>();
            Assert.Equal(2, faturas.Count);
            Assert.Equal(StatusFatura.Fechada, faturas.Single(f => f.IdFatura == faturaAtual.IdFatura).Status);
        }
    }
}
