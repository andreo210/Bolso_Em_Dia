using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CartaoServices;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class CartaoServiceTests
    {
        private static (CartaoService Service, CartaoRepositoryFake Cartoes, NotificadorService Notificador) Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var cartoes = new CartaoRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new CartaoService(cartoes, contas, new UsuarioFake(idUsuario), notificador);

            return (service, cartoes, notificador);
        }

        // ======================= UC13 — Cadastrar cartão de crédito =======================

        [Fact]
        public async Task CriarAsync_com_dados_validos_cria_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, cartoes, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCartaoDto
            {
                Nome = "Nubank",
                LimiteTotal = 3000m,
                DiaFechamento = 5,
                DiaVencimento = 12,
                IdContaPagamento = conta.IdConta
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Nubank", resultado!.Nome);
            Assert.Equal(3000m, resultado.LimiteTotal);
            Assert.Equal(conta.IdConta, resultado.IdContaPagamento);
            Assert.True(resultado.Ativo);
            Assert.Equal(1, cartoes.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var (service, cartoes, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarCartaoDto
            {
                Nome = "Nubank",
                LimiteTotal = 3000m,
                DiaFechamento = 5,
                DiaVencimento = 12,
                IdContaPagamento = 999
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta de pagamento não encontrada");
            Assert.Equal(0, cartoes.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_conta_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var contaDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            armazem.Semear(contaDeOutroUsuario);

            var (service, cartoes, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCartaoDto
            {
                Nome = "Nubank",
                LimiteTotal = 3000m,
                DiaFechamento = 5,
                DiaVencimento = 12,
                IdContaPagamento = contaDeOutroUsuario.IdConta
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta de pagamento não encontrada");
            Assert.Equal(0, cartoes.Salvamentos);
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_cartoes_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            armazem.Semear(
                Fabrica.Cartao(nome: "Meu cartão"),
                Fabrica.Cartao(idUsuario: "outro-usuario", nome: "Cartão de outro"));

            var (service, _, _) = Montar(armazem);

            var cartoes = await service.ObterTodosAsync();

            Assert.Single(cartoes);
            Assert.Equal("Meu cartão", cartoes.Single().Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_cartao_existente_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao(nome: "Nubank");
            armazem.Semear(cartao);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(cartao.IdCartao);

            Assert.NotNull(resultado);
            Assert.Equal("Nubank", resultado!.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_cartao_inexistente_retorna_null()
        {
            var (service, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_de_cartao_de_outro_usuario_retorna_null()
        {
            var armazem = new ArmazemFake();
            var cartaoDeOutroUsuario = Fabrica.Cartao(idUsuario: "outro-usuario");
            armazem.Semear(cartaoDeOutroUsuario);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(cartaoDeOutroUsuario.IdCartao);

            Assert.Null(resultado);
        }
    }
}
