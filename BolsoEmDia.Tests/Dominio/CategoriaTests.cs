using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fabricas;
using Xunit;

namespace BolsoEmDia.Tests.Dominio
{
    public class CategoriaTests
    {
        [Fact]
        public void Criar_com_dados_validos_nasce_ativa()
        {
            var categoria = Categoria.Criar("usuario-1", "Salário", TipoCategoria.Receita);

            Assert.True(categoria.Ativa);
            Assert.Equal("Salário", categoria.Nome);
            Assert.Equal(TipoCategoria.Receita, categoria.Tipo);
            Assert.Null(categoria.IdCategoriaPai);
        }

        [Fact]
        public void Criar_sem_usuario_lanca()
        {
            Assert.Throws<DomainException>(() =>
                Categoria.Criar(" ", "Salário", TipoCategoria.Receita));
        }

        [Fact]
        public void Criar_sem_nome_lanca()
        {
            var erro = Assert.Throws<DomainException>(() =>
                Categoria.Criar("usuario-1", " ", TipoCategoria.Receita));

            Assert.Contains("Nome", erro.Message);
        }

        [Fact]
        public void Renomear_com_nome_vazio_lanca()
        {
            var categoria = Fabrica.Categoria();

            Assert.Throws<DomainException>(() => categoria.Renomear(" "));
        }

        [Fact]
        public void Desativar_inativa_a_categoria()
        {
            var categoria = Fabrica.Categoria(ativa: true);

            categoria.Desativar();

            Assert.False(categoria.Ativa);
        }

        [Fact]
        public void Ativar_reativa_a_categoria()
        {
            var categoria = Fabrica.Categoria(ativa: false);

            categoria.Ativar();

            Assert.True(categoria.Ativa);
        }
    }
}
