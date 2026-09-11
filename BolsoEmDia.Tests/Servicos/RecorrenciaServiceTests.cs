using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.RecorrenciaServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class RecorrenciaServiceTests
    {
        private static (RecorrenciaService Service, ArmazemFake Armazem, RecorrenciaRepositoryFake Recorrencias, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var recorrencias = new RecorrenciaRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var cartoes = new CartaoRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new RecorrenciaService(
                recorrencias, contas, cartoes, categorias, new UsuarioFake(idUsuario), notificador);

            return (service, arm, recorrencias, notificador);
        }

        private static CriarRecorrenciaDto Dto(
            int? idConta = 1, int? idCartao = null, int idCategoria = 1,
            TipoTransacao? tipoTransacao = TipoTransacao.Despesa, decimal valor = 100m,
            FrequenciaRecorrencia frequencia = FrequenciaRecorrencia.Mensal, int diaGeracao = 5,
            DateTime? dataInicio = null, DateTime? dataFim = null)
            => new()
            {
                IdConta = idConta,
                IdCartao = idCartao,
                IdCategoria = idCategoria,
                TipoTransacao = tipoTransacao,
                Valor = valor,
                Frequencia = frequencia,
                DiaGeracao = diaGeracao,
                DataInicio = dataInicio ?? DateTime.UtcNow,
                DataFim = dataFim
            };

        // ======================= UC18 — Criar recorrência =======================

        [Fact]
        public async Task CriarAsync_ligada_a_conta_com_dados_validos_cria_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(idConta: conta.IdConta, idCategoria: categoria.IdCategoria));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.True(resultado!.Ativa);
            Assert.Equal(conta.IdConta, resultado.IdConta);
            Assert.Null(resultado.IdCartao);
            Assert.Single(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_ligada_a_cartao_com_dados_validos_cria_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            armazem.Semear(cartao);
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(
                Dto(idConta: null, idCartao: cartao.IdCartao, idCategoria: categoria.IdCategoria, tipoTransacao: null));

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(cartao.IdCartao, resultado!.IdCartao);
            Assert.Null(resultado.IdConta);
            Assert.Null(resultado.TipoTransacao);
            Assert.Single(arm.Tabela<Recorrencia>());
        }

        // E1 — conta e cartão preenchidos ao mesmo tempo.
        [Fact]
        public async Task CriarAsync_com_conta_e_cartao_preenchidos_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);
            var cartao = Fabrica.Cartao();
            armazem.Semear(cartao);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(idConta: conta.IdConta, idCartao: cartao.IdCartao));

            Assert.Null(resultado);
            notificador.DeveNotificar("exatamente uma conta ou um cartão");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        // E1 — nenhum dos dois preenchido.
        [Fact]
        public async Task CriarAsync_sem_conta_e_sem_cartao_notifica_e_nao_grava()
        {
            var (service, arm, _, notificador) = Montar();

            var resultado = await service.CriarAsync(Dto(idConta: null, idCartao: null));

            Assert.Null(resultado);
            notificador.DeveNotificar("exatamente uma conta ou um cartão");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var (service, arm, _, notificador) = Montar();

            var resultado = await service.CriarAsync(Dto(idConta: 999));

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta não encontrada");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_com_cartao_inexistente_notifica_e_nao_grava()
        {
            var (service, arm, _, notificador) = Montar();

            var resultado = await service.CriarAsync(Dto(idConta: null, idCartao: 999, tipoTransacao: null));

            Assert.Null(resultado);
            notificador.DeveNotificar("Cartão não encontrado");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_ligada_a_conta_sem_tipo_transacao_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(idConta: conta.IdConta, tipoTransacao: null));

            Assert.Null(resultado);
            notificador.DeveNotificar("Tipo da transação deve ser receita ou despesa");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(idConta: conta.IdConta, idCategoria: 999));

            Assert.Null(resultado);
            notificador.DeveNotificar("Categoria não encontrada");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        // Pré-condição: categoria do mesmo tipo compatível com TipoTransacao.
        [Fact]
        public async Task CriarAsync_ligada_a_conta_com_categoria_de_tipo_incompativel_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);
            var categoriaDeReceita = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(categoriaDeReceita);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(
                idConta: conta.IdConta, idCategoria: categoriaDeReceita.IdCategoria, tipoTransacao: TipoTransacao.Despesa));

            Assert.Null(resultado);
            notificador.DeveNotificar("não é uma categoria de despesa");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        // Recorrência em cartão sempre gera compra (UC14), que só aceita categoria de despesa.
        [Fact]
        public async Task CriarAsync_ligada_a_cartao_com_categoria_de_receita_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var cartao = Fabrica.Cartao();
            armazem.Semear(cartao);
            var categoriaDeReceita = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(categoriaDeReceita);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(
                Dto(idConta: null, idCartao: cartao.IdCartao, idCategoria: categoriaDeReceita.IdCategoria, tipoTransacao: null));

            Assert.Null(resultado);
            notificador.DeveNotificar("não é uma categoria de despesa");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        [Fact]
        public async Task CriarAsync_de_conta_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var contaDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            armazem.Semear(contaDeOutroUsuario);

            var (service, arm, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(Dto(idConta: contaDeOutroUsuario.IdConta));

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta não encontrada");
            Assert.Empty(arm.Tabela<Recorrencia>());
        }

        // ======================= UC19 — Pausar / cancelar recorrência =======================

        [Fact]
        public async Task PausarAsync_com_recorrencia_ativa_pausa_e_grava()
        {
            var armazem = new ArmazemFake();
            var recorrencia = Fabrica.Recorrencia();
            armazem.Semear(recorrencia);

            var (service, _, recorrencias, notificador) = Montar(armazem);

            var resultado = await service.PausarAsync(recorrencia.IdRecorrencia);

            notificador.NaoDeveNotificar();
            Assert.True(resultado);
            Assert.False(recorrencia.Ativa);
            Assert.Equal(1, recorrencias.Salvamentos);
        }

        [Fact]
        public async Task ReativarAsync_com_recorrencia_pausada_reativa_e_grava()
        {
            var armazem = new ArmazemFake();
            var recorrencia = Fabrica.Recorrencia();
            recorrencia.Pausar();
            armazem.Semear(recorrencia);

            var (service, _, recorrencias, notificador) = Montar(armazem);

            var resultado = await service.ReativarAsync(recorrencia.IdRecorrencia);

            notificador.NaoDeveNotificar();
            Assert.True(resultado);
            Assert.True(recorrencia.Ativa);
            Assert.Equal(1, recorrencias.Salvamentos);
        }

        [Fact]
        public async Task PausarAsync_com_recorrencia_inexistente_notifica_e_nao_grava()
        {
            var (service, _, recorrencias, notificador) = Montar();

            var resultado = await service.PausarAsync(999);

            Assert.False(resultado);
            notificador.DeveNotificar("Recorrência não encontrada");
            Assert.Equal(0, recorrencias.Salvamentos);
        }

        [Fact]
        public async Task ReativarAsync_com_recorrencia_inexistente_notifica_e_nao_grava()
        {
            var (service, _, recorrencias, notificador) = Montar();

            var resultado = await service.ReativarAsync(999);

            Assert.False(resultado);
            notificador.DeveNotificar("Recorrência não encontrada");
            Assert.Equal(0, recorrencias.Salvamentos);
        }

        [Fact]
        public async Task PausarAsync_de_recorrencia_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var recorrenciaDeOutroUsuario = Fabrica.Recorrencia(idUsuario: "outro-usuario");
            armazem.Semear(recorrenciaDeOutroUsuario);

            var (service, _, recorrencias, notificador) = Montar(armazem);

            var resultado = await service.PausarAsync(recorrenciaDeOutroUsuario.IdRecorrencia);

            Assert.False(resultado);
            notificador.DeveNotificar("Recorrência não encontrada");
            Assert.Equal(0, recorrencias.Salvamentos);
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_recorrencias_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            var recorrencia = Fabrica.Recorrencia();
            armazem.Semear(recorrencia);

            var recorrenciaDeOutroUsuario = Fabrica.Recorrencia(idUsuario: "outro-usuario");
            armazem.Semear(recorrenciaDeOutroUsuario);

            var (service, _, _, _) = Montar(armazem);

            var recorrencias = await service.ObterTodosAsync();

            Assert.Single(recorrencias);
            Assert.Equal(recorrencia.IdRecorrencia, recorrencias.Single().IdRecorrencia);
        }

        [Fact]
        public async Task ObterPorIdAsync_recorrencia_inexistente_retorna_null()
        {
            var (service, _, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }
    }
}
