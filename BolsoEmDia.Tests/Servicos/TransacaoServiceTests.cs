using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.TransacaoServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class TransacaoServiceTests
    {
        private static (TransacaoService Service, TransacaoRepositoryFake Transacoes, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null)
        {
            var arm = armazem ?? new ArmazemFake();
            var transacoes = new TransacaoRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new TransacaoService(transacoes, contas, categorias, new UsuarioFake(), notificador);

            return (service, transacoes, notificador);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_dados_validos_grava_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m,
                Descricao = "Salário"
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(TipoTransacao.Receita, resultado!.Tipo);
            Assert.Equal(150m, resultado.Valor);
            Assert.Equal(conta.IdConta, resultado.IdConta);
            Assert.Equal(categoria.IdCategoria, resultado.IdCategoria);
            Assert.Equal(1, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = 999,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("onta");
            Assert.Equal(0, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = 999,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("ategoria");
            Assert.Equal(0, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_categoria_de_despesa_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            var categoriaDeDespesa = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoriaDeDespesa);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoriaDeDespesa.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("receita");
            Assert.Equal(0, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_conta_e_categoria_invalidas_acumula_as_duas_notificacoes()
        {
            var (service, transacoes, notificador) = Montar();

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = 999,
                IdCategoria = 999,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveTerNotificacoes(2);
            Assert.Equal(0, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarReceitaAsync_com_conta_de_outro_usuario_e_tratada_como_nao_encontrada()
        {
            var armazem = new ArmazemFake();
            var contaDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(contaDeOutroUsuario);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarReceitaAsync(new CriarReceitaDto
            {
                IdConta = contaDeOutroUsuario.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("onta");
            Assert.Equal(0, transacoes.Salvamentos);
        }
    }
}
