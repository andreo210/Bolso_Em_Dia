using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fabricas;
using Xunit;

namespace BolsoEmDia.Tests.Dominio
{
    public class ContaTests
    {
        [Fact]
        public void Criar_com_dados_validos_nasce_ativa()
        {
            var conta = Conta.Criar("usuario-1", "Conta corrente", TipoConta.Corrente, 100m);

            Assert.True(conta.Ativa);
            Assert.Equal("usuario-1", conta.IdUsuario);
            Assert.Equal("Conta corrente", conta.Nome);
            Assert.Equal(TipoConta.Corrente, conta.Tipo);
            Assert.Equal(100m, conta.SaldoInicial);
        }

        [Fact]
        public void Criar_sem_usuario_lanca()
        {
            var erro = Assert.Throws<DomainException>(() =>
                Conta.Criar(" ", "Conta corrente", TipoConta.Corrente, 0m));

            Assert.Contains("suário", erro.Message);
        }

        [Fact]
        public void Criar_sem_nome_lanca()
        {
            var erro = Assert.Throws<DomainException>(() =>
                Conta.Criar("usuario-1", " ", TipoConta.Corrente, 0m));

            Assert.Contains("Nome", erro.Message);
        }

        [Theory]
        [InlineData(TipoConta.Poupanca)]
        [InlineData(TipoConta.Carteira)]
        [InlineData(TipoConta.Investimento)]
        public void Criar_com_saldo_inicial_negativo_fora_de_conta_corrente_lanca(TipoConta tipo)
        {
            var erro = Assert.Throws<DomainException>(() =>
                Conta.Criar("usuario-1", "Conta", tipo, -50m));

            Assert.Contains("corrente", erro.Message);
        }

        [Fact]
        public void Criar_com_saldo_inicial_negativo_em_conta_corrente_permite()
        {
            var conta = Conta.Criar("usuario-1", "Cheque especial", TipoConta.Corrente, -50m);

            Assert.Equal(-50m, conta.SaldoInicial);
        }

        [Fact]
        public void Renomear_com_nome_valido_atualiza()
        {
            var conta = Fabrica.Conta(nome: "Nome antigo");

            conta.Renomear("Nome novo");

            Assert.Equal("Nome novo", conta.Nome);
        }

        [Fact]
        public void Renomear_com_nome_vazio_lanca()
        {
            var conta = Fabrica.Conta();

            var erro = Assert.Throws<DomainException>(() => conta.Renomear(""));

            Assert.Contains("Nome", erro.Message);
        }

        [Fact]
        public void Desativar_inativa_a_conta()
        {
            var conta = Fabrica.Conta(ativa: true);

            conta.Desativar();

            Assert.False(conta.Ativa);
        }

        [Fact]
        public void Ativar_reativa_a_conta()
        {
            var conta = Fabrica.Conta(ativa: false);

            conta.Ativar();

            Assert.True(conta.Ativa);
        }

        [Theory]
        [InlineData(TipoConta.Corrente, true)]
        [InlineData(TipoConta.Poupanca, false)]
        [InlineData(TipoConta.Carteira, false)]
        [InlineData(TipoConta.Investimento, false)]
        public void PermiteSaldoNegativo_e_exclusivo_da_conta_corrente(TipoConta tipo, bool esperado)
        {
            var conta = Fabrica.Conta(tipo: tipo, saldoInicial: 0m);

            Assert.Equal(esperado, conta.PermiteSaldoNegativo());
        }
    }
}
