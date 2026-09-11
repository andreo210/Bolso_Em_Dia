using BolsoEmDia.Application.Services.RecorrenciaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    // ======================= UC21 — Gerar ocorrência de recorrência =======================
    public class RecorrenciaJobServiceTests
    {
        private static (RecorrenciaJobService Service, ArmazemFake Armazem, UnitOfWorkFake UnitOfWork) Montar(ArmazemFake? armazem = null)
        {
            var arm = armazem ?? new ArmazemFake();
            var recorrencias = new RecorrenciaRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var cartoes = new CartaoRepositoryFake(arm);
            var compras = new CompraRepositoryFake(arm);
            var parcelas = new ParcelaRepositoryFake(arm);
            var unitOfWork = new UnitOfWorkFake();

            var service = new RecorrenciaJobService(
                recorrencias, contas, categorias, transacoes, cartoes, compras, parcelas, unitOfWork,
                NullLogger<RecorrenciaJobService>.Instance);

            return (service, arm, unitOfWork);
        }

        [Fact]
        public async Task ProcessarGeracoesAsync_recorrencia_de_despesa_em_conta_no_dia_certo_gera_transacao()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                valor: 50m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            var transacoes = arm.Tabela<Transacao>();
            Assert.Single(transacoes);
            Assert.Equal(TipoTransacao.Despesa, transacoes[0].Tipo);
            Assert.Equal(50m, transacoes[0].Valor);
            Assert.Equal(conta.IdConta, transacoes[0].IdConta);
            Assert.Equal(recorrencia.IdRecorrencia, transacoes[0].IdRecorrencia);
        }

        [Fact]
        public async Task ProcessarGeracoesAsync_recorrencia_de_receita_em_conta_no_dia_certo_gera_transacao()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Receita,
                valor: 3000m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            var transacoes = arm.Tabela<Transacao>();
            Assert.Single(transacoes);
            Assert.Equal(TipoTransacao.Receita, transacoes[0].Tipo);
            Assert.Equal(3000m, transacoes[0].Valor);
        }

        [Fact]
        public async Task ProcessarGeracoesAsync_fora_do_dia_de_geracao_nao_gera_nada()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            // dia de geração é amanhã, não hoje
            var diaGeracao = hoje.AddDays(1).Day;
            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                diaGeracao: diaGeracao, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            Assert.Empty(arm.Tabela<Transacao>());
        }

        // E1 (via A1 de UC21) — saldo insuficiente bloqueia a geração desta ocorrência, sem lançar.
        [Fact]
        public async Task ProcessarGeracoesAsync_com_saldo_insuficiente_nao_gera_e_nao_lanca()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta(tipo: TipoConta.Carteira, saldoInicial: 10m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                valor: 500m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            Assert.Empty(arm.Tabela<Transacao>());
        }

        [Fact]
        public async Task ProcessarGeracoesAsync_recorrencia_em_cartao_no_dia_certo_gera_compra_a_vista()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(limiteTotal: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: null, idCartao: cartao.IdCartao, idCategoria: categoria.IdCategoria, tipoTransacao: null,
                valor: 39.9m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, unitOfWork) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            var compras = arm.Tabela<Compra>();
            Assert.Single(compras);
            Assert.Equal(1, compras[0].NumeroParcelas);
            Assert.Equal(39.9m, compras[0].ValorTotal);
            Assert.Equal(recorrencia.IdRecorrencia, compras[0].IdRecorrencia);
            Assert.Single(arm.Tabela<Parcela>());
            Assert.Equal(1, unitOfWork.Commits);
        }

        // UC15 — mesmo cartão sem limite disponível bloqueia a geração desta ocorrência.
        [Fact]
        public async Task ProcessarGeracoesAsync_recorrencia_em_cartao_sem_limite_disponivel_nao_gera_compra()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var cartao = Fabrica.Cartao(limiteTotal: 10m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: null, idCartao: cartao.IdCartao, idCategoria: categoria.IdCategoria, tipoTransacao: null,
                valor: 39.9m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            Assert.Empty(arm.Tabela<Compra>());
        }

        // UC19/UC21 — DataFim já passada: a recorrência deixa de gerar e é pausada automaticamente.
        [Fact]
        public async Task ProcessarGeracoesAsync_com_data_fim_ja_passada_pausa_a_recorrencia_e_nao_gera()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-2), dataFim: hoje.AddDays(-1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            Assert.Empty(arm.Tabela<Transacao>());
            Assert.False(recorrencia.Ativa);
        }

        // DataInicio ainda não chegou: mesmo que o dia do mês bata, não deve gerar cedo demais.
        [Fact]
        public async Task ProcessarGeracoesAsync_com_data_inicio_no_futuro_nao_gera()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var recorrencia = Fabrica.Recorrencia(
                idConta: conta.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(1));
            armazem.Semear(recorrencia);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            Assert.Empty(arm.Tabela<Transacao>());
        }

        // A1 — uma recorrência bloqueada não pode interromper o processamento do lote.
        [Fact]
        public async Task ProcessarGeracoesAsync_recorrencia_bloqueada_nao_interrompe_as_demais_do_lote()
        {
            var armazem = new ArmazemFake();
            var hoje = DateTime.UtcNow.Date;
            var contaSemSaldo = Fabrica.Conta(tipo: TipoConta.Carteira, saldoInicial: 0m);
            var contaComSaldo = Fabrica.Conta(tipo: TipoConta.Corrente);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(contaSemSaldo);
            armazem.Semear(contaComSaldo);
            armazem.Semear(categoria);

            var bloqueada = Fabrica.Recorrencia(
                idConta: contaSemSaldo.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                valor: 500m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            var valida = Fabrica.Recorrencia(
                idConta: contaComSaldo.IdConta, idCategoria: categoria.IdCategoria, tipoTransacao: TipoTransacao.Despesa,
                valor: 50m, diaGeracao: hoje.Day, dataInicio: hoje.AddMonths(-1));
            armazem.Semear(bloqueada);
            armazem.Semear(valida);

            var (service, arm, _) = Montar(armazem);

            await service.ProcessarGeracoesAsync(hoje);

            var transacoes = arm.Tabela<Transacao>();
            Assert.Single(transacoes);
            Assert.Equal(contaComSaldo.IdConta, transacoes[0].IdConta);
        }
    }
}
