using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.CategoriaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class CategoriaServiceTests
    {
        private static (CategoriaService Service, CategoriaRepositoryFake Categorias, NotificadorService Notificador) Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var categorias = new CategoriaRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new CategoriaService(categorias, new UsuarioFake(idUsuario), notificador);

            return (service, categorias, notificador);
        }

        [Fact]
        public async Task CriarAsync_com_dados_validos_grava_e_retorna_dto()
        {
            var (service, categorias, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Alimentação",
                Tipo = TipoCategoria.Despesa
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Alimentação", resultado!.Nome);
            Assert.Equal(TipoCategoria.Despesa, resultado.Tipo);
            Assert.Null(resultado.IdCategoriaPai);
            Assert.True(resultado.Ativa);
            Assert.Equal(1, categorias.Salvamentos);
            Assert.Equal(Fabrica.IdUsuarioPadrao, categorias.Armazem.Tabela<Categoria>().Single().IdUsuario);
        }

        [Fact]
        public async Task CriarAsync_com_categoria_pai_valida_grava_como_subcategoria()
        {
            var armazem = new ArmazemFake();
            var pai = Fabrica.Categoria(nome: "Alimentação", tipo: TipoCategoria.Despesa);
            armazem.Semear(pai);

            var (service, categorias, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Restaurante",
                Tipo = TipoCategoria.Despesa,
                IdCategoriaPai = pai.IdCategoria
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(pai.IdCategoria, resultado!.IdCategoriaPai);
            Assert.Equal(1, categorias.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_categoria_pai_inexistente_notifica_e_nao_grava()
        {
            var (service, categorias, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Restaurante",
                Tipo = TipoCategoria.Despesa,
                IdCategoriaPai = 999
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("não encontrada");
            Assert.Equal(0, categorias.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_categoria_pai_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var paiDeOutroUsuario = Fabrica.Categoria(idUsuario: "outro-usuario", tipo: TipoCategoria.Despesa);
            armazem.Semear(paiDeOutroUsuario);

            var (service, categorias, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Restaurante",
                Tipo = TipoCategoria.Despesa,
                IdCategoriaPai = paiDeOutroUsuario.IdCategoria
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("não encontrada");
            Assert.Equal(0, categorias.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_categoria_pai_de_tipo_diferente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var pai = Fabrica.Categoria(nome: "Salário", tipo: TipoCategoria.Receita);
            armazem.Semear(pai);

            var (service, categorias, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Restaurante",
                Tipo = TipoCategoria.Despesa,
                IdCategoriaPai = pai.IdCategoria
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("mesmo tipo");
            Assert.Equal(0, categorias.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_categoria_pai_que_ja_e_subcategoria_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var avo = Fabrica.Categoria(nome: "Alimentação", tipo: TipoCategoria.Despesa);
            armazem.Semear(avo);
            var pai = Fabrica.Categoria(nome: "Restaurante", tipo: TipoCategoria.Despesa, idCategoriaPai: avo.IdCategoria);
            armazem.Semear(pai);

            var (service, categorias, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarCategoriaDto
            {
                Nome = "Fast-food",
                Tipo = TipoCategoria.Despesa,
                IdCategoriaPai = pai.IdCategoria
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("subcategoria");
            Assert.Equal(0, categorias.Salvamentos);
        }

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_categorias_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            armazem.Semear(
                Fabrica.Categoria(nome: "Minha categoria"),
                Fabrica.Categoria(nome: "Categoria de outro usuário", idUsuario: "outro-usuario"));

            var (service, _, _) = Montar(armazem);

            var categorias = await service.ObterTodosAsync();

            Assert.Single(categorias);
            Assert.Equal("Minha categoria", categorias.Single().Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_categoria_existente_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(nome: "Transporte", tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(categoria.IdCategoria);

            Assert.NotNull(resultado);
            Assert.Equal("Transporte", resultado!.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_categoria_inexistente_retorna_null()
        {
            var (service, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_de_categoria_de_outro_usuario_retorna_null()
        {
            var armazem = new ArmazemFake();
            var categoriaDeOutroUsuario = Fabrica.Categoria(idUsuario: "outro-usuario");
            armazem.Semear(categoriaDeOutroUsuario);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(categoriaDeOutroUsuario.IdCategoria);

            Assert.Null(resultado);
        }
    }
}
