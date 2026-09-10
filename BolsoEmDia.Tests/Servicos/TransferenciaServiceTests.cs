using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.TransferenciaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using System.Linq;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    // UC06 — Transferir entre contas (inclui UC05)
    public class TransferenciaServiceTests
    {
        private static (TransferenciaService Service, TransferenciaRepositoryFake Transferencias,
            TransacaoRepositoryFake Transacoes, UnitOfWorkFake UnitOfWork, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null)
        {
            var arm = armazem ?? new ArmazemFake();
            var transferencias = new TransferenciaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var unitOfWork = new UnitOfWorkFake();
            var notificador = new NotificadorService();

            var service = new TransferenciaService(
                transferencias, transacoes, contas, unitOfWork, new UsuarioFake(), notificador);

            return (service, transferencias, transacoes, unitOfWork, notificador);
        }

        [Fact]
        public async Task RegistrarAsync_com_dados_validos_grava_as_duas_pernas_atomicas_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var origem = Fabrica.Conta(nome: "Corrente", saldoInicial: 1000m);
            var destino = Fabrica.Conta(nome: "Poupança", tipo: TipoConta.Poupanca);
            armazem.Semear(origem);
            armazem.Semear(destino);

            var (service, transferencias, transacoes, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = origem.IdConta,
                IdContaDestino = destino.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m,
                Descricao = "Reserva"
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(origem.IdConta, resultado!.IdContaOrigem);
            Assert.Equal(destino.IdConta, resultado.IdContaDestino);
            Assert.Equal(150m, resultado.Valor);

            Assert.Single(transferencias.ObterTodos());
            Assert.Equal(1, unitOfWork.Commits);
            Assert.Equal(0, unitOfWork.Rollbacks);

            var pernas = transacoes.ObterTodos().ToList();
            Assert.Equal(2, pernas.Count);

            var saida = Assert.Single(pernas, p => p.Tipo == TipoTransacao.TransferenciaSaida);
            Assert.Equal(origem.IdConta, saida.IdConta);
            Assert.Equal(150m, saida.Valor);
            Assert.Null(saida.IdCategoria);

            var entrada = Assert.Single(pernas, p => p.Tipo == TipoTransacao.TransferenciaEntrada);
            Assert.Equal(destino.IdConta, entrada.IdConta);
            Assert.Equal(150m, entrada.Valor);
            Assert.Null(entrada.IdCategoria);
        }

        [Fact]
        public async Task RegistrarAsync_com_conta_origem_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var destino = Fabrica.Conta();
            armazem.Semear(destino);

            var (service, transferencias, transacoes, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = 999,
                IdContaDestino = destino.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("origem");
            Assert.Empty(transferencias.ObterTodos());
            Assert.Empty(transacoes.ObterTodos());
            Assert.False(unitOfWork.HasActiveTransaction);
        }

        [Fact]
        public async Task RegistrarAsync_com_conta_destino_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var origem = Fabrica.Conta();
            armazem.Semear(origem);

            var (service, transferencias, transacoes, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = origem.IdConta,
                IdContaDestino = 999,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("destino");
            Assert.Empty(transferencias.ObterTodos());
            Assert.Empty(transacoes.ObterTodos());
        }

        [Fact]
        public async Task RegistrarAsync_com_conta_origem_e_destino_iguais_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, transferencias, transacoes, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = conta.IdConta,
                IdContaDestino = conta.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("mesma");
            Assert.Empty(transferencias.ObterTodos());
            Assert.Empty(transacoes.ObterTodos());
        }

        [Fact]
        public async Task RegistrarAsync_com_conta_de_outro_usuario_e_tratada_como_nao_encontrada()
        {
            var armazem = new ArmazemFake();
            var origemDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            var destino = Fabrica.Conta();
            armazem.Semear(origemDeOutroUsuario);
            armazem.Semear(destino);

            var (service, _, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = origemDeOutroUsuario.IdConta,
                IdContaDestino = destino.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("origem");
        }

        // E1 — saldo insuficiente numa origem que não permite negativar (Poupança/Carteira/Investimento).
        [Fact]
        public async Task RegistrarAsync_com_origem_poupanca_e_saldo_insuficiente_bloqueia_as_duas_pernas()
        {
            var armazem = new ArmazemFake();
            var origem = Fabrica.Conta(tipo: TipoConta.Poupanca, saldoInicial: 100m);
            var destino = Fabrica.Conta(nome: "Destino");
            armazem.Semear(origem);
            armazem.Semear(destino);

            var (service, transferencias, transacoes, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = origem.IdConta,
                IdContaDestino = destino.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Saldo insuficiente");
            Assert.Empty(transferencias.ObterTodos());
            Assert.Empty(transacoes.ObterTodos());
            Assert.Equal(0, unitOfWork.Commits);
        }

        // Conta corrente sempre permite negativar (cheque especial) — nunca dispara E1.
        [Fact]
        public async Task RegistrarAsync_com_origem_corrente_permite_saldo_resultante_negativo()
        {
            var armazem = new ArmazemFake();
            var origem = Fabrica.Conta(tipo: TipoConta.Corrente, saldoInicial: 100m);
            var destino = Fabrica.Conta(nome: "Destino");
            armazem.Semear(origem);
            armazem.Semear(destino);

            var (service, transferencias, _, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(new CriarTransferenciaDto
            {
                IdContaOrigem = origem.IdConta,
                IdContaDestino = destino.IdConta,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Single(transferencias.ObterTodos());
        }
    }
}
