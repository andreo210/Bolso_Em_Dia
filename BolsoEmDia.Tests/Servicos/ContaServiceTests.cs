using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.ContaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class ContaServiceTests
    {
        private static (ContaService Service, ContaRepositoryFake Contas, TransacaoRepositoryFake Transacoes, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var contas = new ContaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new ContaService(contas, transacoes, new UsuarioFake(idUsuario), notificador);

            return (service, contas, transacoes, notificador);
        }

        [Fact]
        public async Task CriarAsync_com_dados_validos_grava_e_retorna_dto()
        {
            var (service, contas, _, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarContaDto
            {
                Nome = "Conta corrente",
                Tipo = TipoConta.Corrente,
                SaldoInicial = 100m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Conta corrente", resultado!.Nome);
            Assert.Equal(100m, resultado.SaldoInicial);
            Assert.True(resultado.Ativa);
            Assert.Equal(1, contas.Salvamentos);
            Assert.Equal(Fabrica.IdUsuarioPadrao, contas.Armazem.Tabela<Conta>().Single().IdUsuario);
        }

        [Theory]
        [InlineData(TipoConta.Poupanca)]
        [InlineData(TipoConta.Carteira)]
        [InlineData(TipoConta.Investimento)]
        public async Task CriarAsync_com_saldo_inicial_negativo_fora_de_conta_corrente_notifica_e_nao_grava(TipoConta tipo)
        {
            var (service, contas, _, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarContaDto
            {
                Nome = "Poupança",
                Tipo = tipo,
                SaldoInicial = -10m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("corrente");
            Assert.Equal(0, contas.Salvamentos);
        }

        [Fact]
        public async Task ObterSaldoAsync_soma_saldo_inicial_com_transacoes_efetivadas_e_ignora_agendamentos_futuros()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 100m);
            armazem.Semear(conta);

            armazem.Semear(
                Fabrica.Receita(idConta: conta.IdConta, valor: 50m, data: DateTime.UtcNow),
                Fabrica.Despesa(idConta: conta.IdConta, valor: 20m, data: DateTime.UtcNow),
                Fabrica.Receita(idConta: conta.IdConta, valor: 1000m, data: Fabrica.DaquiADias(3)));

            var (service, _, _, _) = Montar(armazem);

            var saldo = await service.ObterSaldoAsync(conta.IdConta);

            Assert.NotNull(saldo);
            Assert.Equal(130m, saldo!.Saldo);
        }

        [Fact]
        public async Task ObterSaldoAsync_conta_inexistente_retorna_null()
        {
            var (service, _, _, _) = Montar();

            var saldo = await service.ObterSaldoAsync(999);

            Assert.Null(saldo);
        }

        [Fact]
        public async Task ObterSaldoAsync_de_conta_de_outro_usuario_retorna_null()
        {
            var armazem = new ArmazemFake();
            var contaDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            armazem.Semear(contaDeOutroUsuario);

            var (service, _, _, _) = Montar(armazem);

            var saldo = await service.ObterSaldoAsync(contaDeOutroUsuario.IdConta);

            Assert.Null(saldo);
        }

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_contas_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            armazem.Semear(
                Fabrica.Conta(nome: "Minha conta"),
                Fabrica.Conta(nome: "Conta de outro usuário", idUsuario: "outro-usuario"));

            var (service, _, _, _) = Montar(armazem);

            var contas = await service.ObterTodosAsync();

            Assert.Single(contas);
            Assert.Equal("Minha conta", contas.Single().Nome);
        }

        [Fact]
        public async Task AtualizarAsync_com_conta_existente_renomeia_e_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(nome: "Nome antigo");
            armazem.Semear(conta);

            var (service, contas, _, notificador) = Montar(armazem);

            var resultado = await service.AtualizarAsync(conta.IdConta, new AtualizarContaDto { Nome = "Nome novo" });

            notificador.NaoDeveNotificar();
            Assert.True(resultado);
            Assert.Equal("Nome novo", conta.Nome);
            Assert.Equal(1, contas.Salvamentos);
        }

        [Fact]
        public async Task AtualizarAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var (service, contas, _, notificador) = Montar();

            var resultado = await service.AtualizarAsync(999, new AtualizarContaDto { Nome = "Nome novo" });

            Assert.False(resultado);
            notificador.DeveNotificar("encontrada");
            Assert.Equal(0, contas.Salvamentos);
        }

        [Fact]
        public async Task InativarAsync_com_conta_ativa_desativa_e_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(ativa: true);
            armazem.Semear(conta);

            var (service, contas, _, notificador) = Montar(armazem);

            var resultado = await service.InativarAsync(conta.IdConta);

            notificador.NaoDeveNotificar();
            Assert.True(resultado);
            Assert.False(conta.Ativa);
            Assert.Equal(1, contas.Salvamentos);
        }

        [Fact]
        public async Task AtivarAsync_com_conta_inativa_ativa_e_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(ativa: false);
            armazem.Semear(conta);

            var (service, contas, _, notificador) = Montar(armazem);

            var resultado = await service.AtivarAsync(conta.IdConta);

            notificador.NaoDeveNotificar();
            Assert.True(resultado);
            Assert.True(conta.Ativa);
            Assert.Equal(1, contas.Salvamentos);
        }

        [Fact]
        public async Task AtivarAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var (service, contas, _, notificador) = Montar();

            var resultado = await service.AtivarAsync(999);

            Assert.False(resultado);
            notificador.DeveNotificar("encontrada");
            Assert.Equal(0, contas.Salvamentos);
        }
    }
}
