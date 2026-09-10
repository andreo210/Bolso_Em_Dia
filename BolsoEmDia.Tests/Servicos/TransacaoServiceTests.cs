using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.OrcamentoServices;
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
            var orcamentos = new OrcamentoRepositoryFake(arm);
            var notificador = new NotificadorService();

            // OrcamentoService real (não fake): UC04 usa VerificarEstouroAsync de verdade, e é isso
            // que os testes de UC10 abaixo precisam exercitar — um fake esconderia a integração.
            var orcamentoService = new OrcamentoService(orcamentos, categorias, transacoes, new UsuarioFake(), notificador);

            var service = new TransacaoService(transacoes, contas, categorias, orcamentoService, new UsuarioFake(), notificador);

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

        // ======================= UC04 — Registrar despesa =======================

        [Fact]
        public async Task RegistrarDespesaAsync_com_dados_validos_grava_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m,
                Descricao = "Mercado"
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(TipoTransacao.Despesa, resultado!.Tipo);
            Assert.Equal(150m, resultado.Valor);
            Assert.Null(resultado.AlertaOrcamento);
            Assert.Equal(1, transacoes.Salvamentos);
        }

        [Fact]
        public async Task RegistrarDespesaAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
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
        public async Task RegistrarDespesaAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
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
        public async Task RegistrarDespesaAsync_com_categoria_de_receita_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            var categoriaDeReceita = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(conta);
            armazem.Semear(categoriaDeReceita);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoriaDeReceita.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("despesa");
            Assert.Equal(0, transacoes.Salvamentos);
        }

        // E1 — saldo insuficiente numa conta que não permite negativar (Poupança/Carteira/Investimento).
        [Fact]
        public async Task RegistrarDespesaAsync_em_conta_poupanca_com_saldo_insuficiente_bloqueia_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(tipo: TipoConta.Poupanca, saldoInicial: 100m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Saldo insuficiente");
            Assert.Equal(0, transacoes.Salvamentos);
        }

        // Conta corrente sempre permite negativar (cheque especial) — nunca dispara E1.
        [Fact]
        public async Task RegistrarDespesaAsync_em_conta_corrente_permite_saldo_resultante_negativo()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(tipo: TipoConta.Corrente, saldoInicial: 100m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(1, transacoes.Salvamentos);
        }

        // A1 — orçamento estourado não bloqueia: a despesa é aceita e o alerta vem no DTO, fora do notificador.
        [Fact]
        public async Task RegistrarDespesaAsync_quando_orcamento_da_categoria_estoura_grava_e_preenche_alerta()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            var categoria = Fabrica.Categoria(nome: "Alimentação", tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 100m);
            armazem.Semear(orcamento);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(1, transacoes.Salvamentos);
            Assert.NotNull(resultado!.AlertaOrcamento);
            Assert.Contains("Alimentação", resultado.AlertaOrcamento);
        }

        [Fact]
        public async Task RegistrarDespesaAsync_sem_orcamento_definido_para_a_categoria_nao_preenche_alerta()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta(saldoInicial: 1000m);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(conta);
            armazem.Semear(categoria);

            var (service, transacoes, notificador) = Montar(armazem);

            var resultado = await service.RegistrarDespesaAsync(new CriarDespesaDto
            {
                IdConta = conta.IdConta,
                IdCategoria = categoria.IdCategoria,
                Data = DateTime.UtcNow,
                Valor = 150m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Null(resultado!.AlertaOrcamento);
        }
    }
}
