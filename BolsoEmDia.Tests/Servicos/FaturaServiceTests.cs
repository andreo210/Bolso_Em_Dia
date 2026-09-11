using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CompraServices;
using BolsoEmDia.Application.Services.FaturaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class FaturaServiceTests
    {
        private static (FaturaService Service, ArmazemFake Armazem, FaturaRepositoryFake Faturas, ParcelaRepositoryFake Parcelas, UnitOfWorkFake UnitOfWork, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var faturas = new FaturaRepositoryFake(arm);
            var cartoes = new CartaoRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var parcelas = new ParcelaRepositoryFake(arm);
            var unitOfWork = new UnitOfWorkFake();
            var notificador = new NotificadorService();

            var service = new FaturaService(
                faturas, cartoes, contas, categorias, transacoes, unitOfWork, new UsuarioFake(idUsuario), notificador);

            return (service, arm, faturas, parcelas, unitOfWork, notificador);
        }

        private static PagarFaturaDto Dto(int idCategoria) => new() { IdCategoria = idCategoria };

        // ======================= UC17 — Pagar fatura =======================

        [Fact]
        public async Task RegistrarPagamentoAsync_fatura_fechada_com_dados_validos_paga_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, _, _, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoria.IdCategoria));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(StatusFatura.Paga, resultado!.Status);
            Assert.NotNull(resultado.IdTransacaoPagamento);
            Assert.Equal(1, unitOfWork.Commits);

            var transacaoGerada = Assert.Single(arm.Tabela<Transacao>());
            Assert.Equal(TipoTransacao.Despesa, transacaoGerada.Tipo);
            Assert.Equal(300m, transacaoGerada.Valor);
            Assert.Equal(conta.IdConta, transacaoGerada.IdConta);
            Assert.Equal(transacaoGerada.IdTransacao, resultado.IdTransacaoPagamento);
        }

        // Aberta -> Paga é pagamento antecipado (ver diagrama-estados.md).
        [Fact]
        public async Task RegistrarPagamentoAsync_fatura_aberta_permite_pagamento_antecipado()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: false);
            armazem.Semear(fatura);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, _, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoria.IdCategoria));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(StatusFatura.Paga, resultado!.Status);

            // Paga não aceita mais lançamento — uma compra nova nesse ciclo desliza pro próximo.
            Assert.False(fatura.AceitaNovoLancamento());
        }

        // E1 — fatura já paga.
        [Fact]
        public async Task RegistrarPagamentoAsync_fatura_ja_paga_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            fatura.RegistrarPagamento(idTransacaoPagamento: 1);
            armazem.Semear(fatura);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, _, _, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Fatura já está paga");
            Assert.Empty(arm.Tabela<Transacao>());
            Assert.Equal(0, unitOfWork.Commits);
        }

        [Fact]
        public async Task RegistrarPagamentoAsync_fatura_inexistente_notifica_e_nao_grava()
        {
            var (service, _, _, _, unitOfWork, notificador) = Montar();

            var resultado = await service.RegistrarPagamentoAsync(999, Dto(1));

            Assert.Null(resultado);
            notificador.DeveNotificar("Fatura não encontrada");
            Assert.Equal(0, unitOfWork.Commits);
        }

        [Fact]
        public async Task RegistrarPagamentoAsync_de_fatura_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(idUsuario: "outro-usuario", saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartaoDeOutroUsuario = Fabrica.Cartao(idUsuario: "outro-usuario", idContaPagamento: conta.IdConta);
            armazem.Semear(cartaoDeOutroUsuario);
            var fatura = Fabrica.Fatura(cartaoDeOutroUsuario, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);

            var (service, _, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(1));

            Assert.Null(resultado);
            notificador.DeveNotificar("Fatura não encontrada");
        }

        [Fact]
        public async Task RegistrarPagamentoAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);

            var (service, arm, _, _, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(999));

            Assert.Null(resultado);
            notificador.DeveNotificar("Categoria não encontrada");
            Assert.Empty(arm.Tabela<Transacao>());
            Assert.Equal(0, unitOfWork.Commits);
        }

        [Fact]
        public async Task RegistrarPagamentoAsync_com_categoria_de_receita_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);
            var categoriaDeReceita = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(categoriaDeReceita);

            var (service, arm, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoriaDeReceita.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("não é uma categoria de despesa");
            Assert.Empty(arm.Tabela<Transacao>());
        }

        [Fact]
        public async Task RegistrarPagamentoAsync_com_categoria_inativa_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);
            var categoriaInativa = Fabrica.Categoria(tipo: TipoCategoria.Despesa, ativa: false);
            armazem.Semear(categoriaInativa);

            var (service, arm, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoriaInativa.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Categoria inativa");
            Assert.Empty(arm.Tabela<Transacao>());
        }

        // E2 — saldo insuficiente numa conta que não permite negativar (Poupança/Carteira/Investimento).
        [Fact]
        public async Task RegistrarPagamentoAsync_em_conta_poupanca_com_saldo_insuficiente_bloqueia_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(tipo: TipoConta.Poupanca, saldoInicial: 100m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, _, _, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Saldo insuficiente na conta de pagamento");
            Assert.Empty(arm.Tabela<Transacao>());
            Assert.Equal(0, unitOfWork.Commits);
            Assert.Equal(StatusFatura.Fechada, fatura.Status);
        }

        // Conta corrente sempre permite negativar (cheque especial) — nunca dispara E2.
        [Fact]
        public async Task RegistrarPagamentoAsync_em_conta_corrente_permite_saldo_resultante_negativo()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(tipo: TipoConta.Corrente, saldoInicial: 100m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao, valorTotal: 300m, fechada: true);
            armazem.Semear(fatura);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, _, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoria.IdCategoria));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(StatusFatura.Paga, resultado!.Status);
        }

        // Pagar a fatura não apaga parcela nenhuma — só deixa de contar como "não paga" na soma
        // que Cartao.LimiteDisponivel usa (ver cartao-de-credito.md).
        [Fact]
        public async Task RegistrarPagamentoAsync_libera_limite_do_cartao_para_as_parcelas_da_fatura_paga()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao(limiteTotal: 500m, idContaPagamento: conta.IdConta);
            armazem.Semear(cartao);
            var categoriaDespesa = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoriaDespesa);

            var compraService = new CompraService(
                new CompraRepositoryFake(armazem), new CartaoRepositoryFake(armazem), new CategoriaRepositoryFake(armazem),
                new ParcelaRepositoryFake(armazem), new UnitOfWorkFake(), new UsuarioFake(), new NotificadorService());

            var compra = await compraService.RegistrarAsync(new CriarCompraDto
            {
                IdCartao = cartao.IdCartao,
                IdCategoria = categoriaDespesa.IdCategoria,
                Data = DateTime.UtcNow,
                Descricao = "Compra à vista",
                ValorTotal = 500m,
                NumeroParcelas = 1
            });
            Assert.NotNull(compra);

            var fatura = cartao.Faturas.Single();
            fatura.Fechar();

            var (service, _, _, parcelas, _, notificador) = Montar(armazem);

            var totalAntesDoPagamento = await parcelas.ObterTotalParcelasNaoPagasAsync(cartao.IdCartao);
            Assert.Equal(500m, totalAntesDoPagamento);

            var resultado = await service.RegistrarPagamentoAsync(fatura.IdFatura, Dto(categoriaDespesa.IdCategoria));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);

            var totalDepoisDoPagamento = await parcelas.ObterTotalParcelasNaoPagasAsync(cartao.IdCartao);
            Assert.Equal(0m, totalDepoisDoPagamento);
            Assert.Equal(500m, cartao.LimiteDisponivel(totalDepoisDoPagamento));
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_faturas_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            armazem.Semear(cartao);
            var fatura = Fabrica.Fatura(cartao);
            armazem.Semear(fatura);

            var cartaoDeOutroUsuario = Fabrica.Cartao(idUsuario: "outro-usuario");
            armazem.Semear(cartaoDeOutroUsuario);
            var faturaDeOutroUsuario = Fabrica.Fatura(cartaoDeOutroUsuario);
            armazem.Semear(faturaDeOutroUsuario);

            var (service, _, _, _, _, _) = Montar(armazem);

            var faturas = await service.ObterTodosAsync();

            Assert.Single(faturas);
            Assert.Equal(fatura.IdFatura, faturas.Single().IdFatura);
        }

        [Fact]
        public async Task ObterPorIdAsync_fatura_inexistente_retorna_null()
        {
            var (service, _, _, _, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }
    }
}
