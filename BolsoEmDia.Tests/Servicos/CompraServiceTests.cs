using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CompraServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class CompraServiceTests
    {
        private static (CompraService Service, ArmazemFake Armazem, UnitOfWorkFake UnitOfWork, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var compras = new CompraRepositoryFake(arm);
            var cartoes = new CartaoRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var parcelas = new ParcelaRepositoryFake(arm);
            var unitOfWork = new UnitOfWorkFake();
            var notificador = new NotificadorService();

            var service = new CompraService(compras, cartoes, categorias, parcelas, unitOfWork, new UsuarioFake(idUsuario), notificador);

            return (service, arm, unitOfWork, notificador);
        }

        private static CriarCompraDto Dto(
            int idCartao, int idCategoria, decimal valorTotal = 100m, int numeroParcelas = 1, string descricao = "Compra de teste")
            => new()
            {
                IdCartao = idCartao,
                IdCategoria = idCategoria,
                Data = DateTime.UtcNow,
                Descricao = descricao,
                ValorTotal = valorTotal,
                NumeroParcelas = numeroParcelas
            };

        // ======================= UC14 — Registrar compra no cartão =======================

        [Fact]
        public async Task RegistrarAsync_a_vista_com_dados_validos_registra_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao(limiteTotal: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria, valorTotal: 300m));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(300m, resultado!.ValorTotal);
            Assert.Single(resultado.Parcelas);
            Assert.Equal(300m, resultado.Parcelas[0].Valor);
            Assert.Equal(1, unitOfWork.Commits);
            Assert.Single(arm.Tabela<Compra>());
            Assert.Single(arm.Tabela<Parcela>());
        }

        [Fact]
        public async Task RegistrarAsync_parcelado_gera_N_parcelas_em_faturas_consecutivas_com_arredondamento_correto()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao(limiteTotal: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria, valorTotal: 100m, numeroParcelas: 3));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado!.Parcelas.Count);
            Assert.Equal(100m, resultado.Parcelas.Sum(p => p.Valor));
            Assert.Equal(new[] { 1, 2, 3 }, resultado.Parcelas.Select(p => p.Numero));

            // 3 parcelas em cartão sem fatura prévia => 3 faturas abertas, uma por ciclo
            Assert.Equal(3, cartao.Faturas.Count);
            Assert.Equal(100m, cartao.Faturas.Sum(f => f.ValorTotal));
            Assert.Equal(3, arm.Tabela<Parcela>().Count);
        }

        [Fact]
        public async Task RegistrarAsync_com_cartao_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(999, categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Cartão não encontrado");
            Assert.Empty(arm.Tabela<Compra>());
            Assert.Equal(0, unitOfWork.Commits);
        }

        [Fact]
        public async Task RegistrarAsync_com_cartao_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartaoDeOutroUsuario = Fabrica.Cartao(idUsuario: "outro-usuario");
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartaoDeOutroUsuario);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartaoDeOutroUsuario.IdCartao, categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Cartão não encontrado");
            Assert.Empty(arm.Tabela<Compra>());
        }

        [Fact]
        public async Task RegistrarAsync_com_cartao_inativo_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            cartao.Desativar();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Cartão inativo");
            Assert.Empty(arm.Tabela<Compra>());
        }

        [Fact]
        public async Task RegistrarAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            armazem.Semear(cartao);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, 999));

            Assert.Null(resultado);
            notificador.DeveNotificar("Categoria não encontrada");
            Assert.Empty(arm.Tabela<Compra>());
        }

        [Fact]
        public async Task RegistrarAsync_com_categoria_de_receita_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("não é uma categoria de despesa");
            Assert.Empty(arm.Tabela<Compra>());
        }

        [Fact]
        public async Task RegistrarAsync_com_categoria_inativa_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa, ativa: false);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria));

            Assert.Null(resultado);
            notificador.DeveNotificar("Categoria inativa");
            Assert.Empty(arm.Tabela<Compra>());
        }

        // E1 — valor total maior que o limite disponível
        [Fact]
        public async Task RegistrarAsync_com_valor_acima_do_limite_disponivel_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao(limiteTotal: 500m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, arm, unitOfWork, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria, valorTotal: 500.01m));

            Assert.Null(resultado);
            notificador.DeveNotificar("Limite disponível insuficiente");
            Assert.Empty(arm.Tabela<Compra>());
            Assert.Equal(0, unitOfWork.Commits);
        }

        // UC15 — soma TODAS as parcelas futuras não pagas, não só a fatura aberta atual
        [Fact]
        public async Task RegistrarAsync_considera_parcelas_de_compra_anterior_no_limite_disponivel()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao(limiteTotal: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, _, _, notificador) = Montar(armazem);

            var primeira = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria, valorTotal: 700m));
            notificador.NaoDeveNotificar();
            Assert.NotNull(primeira);

            // resta 300 de limite — 400 deveria ser bloqueado
            var segunda = await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria, valorTotal: 400m));

            Assert.Null(segunda);
            notificador.DeveNotificar("Limite disponível insuficiente");
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_compras_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(cartao);
            armazem.Semear(categoria);

            var (service, _, _, notificador) = Montar(armazem);
            await service.RegistrarAsync(Dto(cartao.IdCartao, categoria.IdCategoria));
            notificador.NaoDeveNotificar();

            var (serviceOutroUsuario, _, _, _) = Montar(armazem, idUsuario: "outro-usuario");
            var comprasDoOutroUsuario = await serviceOutroUsuario.ObterTodosAsync();
            Assert.Empty(comprasDoOutroUsuario);

            var compras = await service.ObterTodosAsync();
            Assert.Single(compras);
        }

        [Fact]
        public async Task ObterPorIdAsync_compra_inexistente_retorna_null()
        {
            var (service, _, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }
    }
}
