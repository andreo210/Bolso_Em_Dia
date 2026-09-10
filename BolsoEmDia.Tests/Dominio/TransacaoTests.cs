using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fabricas;
using Xunit;

namespace BolsoEmDia.Tests.Dominio
{
    public class TransacaoTests
    {
        [Fact]
        public void RegistrarReceita_com_dados_validos_cria_transacao_do_tipo_receita()
        {
            var data = Fabrica.DiasAtras(1);

            var transacao = Transacao.RegistrarReceita("usuario-1", idConta: 1, idCategoria: 2, data, valor: 150m, descricao: "Salário");

            Assert.Equal(TipoTransacao.Receita, transacao.Tipo);
            Assert.Equal("usuario-1", transacao.IdUsuario);
            Assert.Equal(1, transacao.IdConta);
            Assert.Equal(2, transacao.IdCategoria);
            Assert.Equal(150m, transacao.Valor);
            Assert.Equal("Salário", transacao.Descricao);
            Assert.Null(transacao.IdTransferencia);
        }

        [Fact]
        public void RegistrarDespesa_com_dados_validos_cria_transacao_do_tipo_despesa()
        {
            var transacao = Transacao.RegistrarDespesa("usuario-1", idConta: 1, idCategoria: 3, DateTime.UtcNow, valor: 80m, descricao: "Mercado");

            Assert.Equal(TipoTransacao.Despesa, transacao.Tipo);
        }

        [Fact]
        public void RegistrarReceita_sem_usuario_lanca()
        {
            var erro = Assert.Throws<DomainException>(() =>
                Transacao.RegistrarReceita(" ", 1, 2, DateTime.UtcNow, 100m, null));

            Assert.Contains("suário", erro.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void RegistrarReceita_com_valor_nao_positivo_lanca(decimal valor)
        {
            var erro = Assert.Throws<DomainException>(() =>
                Transacao.RegistrarReceita("usuario-1", 1, 2, DateTime.UtcNow, valor, null));

            Assert.Contains("Valor", erro.Message);
        }

        [Fact]
        public void Editar_atualiza_data_valor_categoria_e_descricao()
        {
            var transacao = Fabrica.Receita(valor: 100m);
            var novaData = Fabrica.DiasAtras(2);

            transacao.Editar(novaData, 200m, idCategoria: 9, descricao: "Ajustado");

            Assert.Equal(novaData, transacao.Data);
            Assert.Equal(200m, transacao.Valor);
            Assert.Equal(9, transacao.IdCategoria);
            Assert.Equal("Ajustado", transacao.Descricao);
        }

        [Fact]
        public void Editar_com_valor_nao_positivo_lanca()
        {
            var transacao = Fabrica.Receita();

            Assert.Throws<DomainException>(() => transacao.Editar(DateTime.UtcNow, 0m, null, null));
        }

        [Fact]
        public void EstaEfetivada_com_data_de_hoje_ou_passada_retorna_true()
        {
            var transacao = Fabrica.Receita(data: DateTime.UtcNow.Date);

            Assert.True(transacao.EstaEfetivada(DateTime.UtcNow.Date));
            Assert.True(transacao.EstaEfetivada(Fabrica.DaquiADias(1)));
        }

        [Fact]
        public void EstaEfetivada_com_data_futura_retorna_false()
        {
            var transacao = Fabrica.Receita(data: Fabrica.DaquiADias(5));

            Assert.False(transacao.EstaEfetivada(DateTime.UtcNow.Date));
        }
    }
}
