using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.MetaEconomiaServices;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class MetaEconomiaServiceTests
    {
        private static (MetaEconomiaService Service, MetaEconomiaRepositoryFake Metas, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var metas = new MetaEconomiaRepositoryFake(arm);
            var contas = new ContaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new MetaEconomiaService(metas, contas, transacoes, new UsuarioFake(idUsuario), notificador);

            return (service, metas, notificador);
        }

        // ======================= UC11 — Criar meta de economia =======================

        [Fact]
        public async Task CriarAsync_com_dados_validos_sem_conta_cria_e_retorna_dto()
        {
            var (service, metas, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarMetaEconomiaDto
            {
                Nome = "Viagem",
                ValorAlvo = 1000m,
                DataAlvo = Fabrica.DaquiADias(90)
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Viagem", resultado!.Nome);
            Assert.Equal(1000m, resultado.ValorAlvo);
            Assert.False(resultado.Concluida);
            Assert.Equal(0m, resultado.TotalAportado);
            Assert.Equal(1, metas.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_conta_existente_do_usuario_cria_meta_vinculada()
        {
            var armazem = new ArmazemFake();
            var conta = Fabrica.Conta();
            armazem.Semear(conta);

            var (service, _, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarMetaEconomiaDto
            {
                Nome = "Reserva",
                ValorAlvo = 500m,
                IdConta = conta.IdConta
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(conta.IdConta, resultado!.IdConta);
        }

        [Fact]
        public async Task CriarAsync_com_conta_inexistente_notifica_e_nao_grava()
        {
            var (service, metas, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarMetaEconomiaDto
            {
                Nome = "Reserva",
                ValorAlvo = 500m,
                IdConta = 999
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta não encontrada");
            Assert.Equal(0, metas.Salvamentos);
        }

        [Fact]
        public async Task CriarAsync_com_conta_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var contaDeOutroUsuario = Fabrica.Conta(idUsuario: "outro-usuario");
            armazem.Semear(contaDeOutroUsuario);

            var (service, metas, notificador) = Montar(armazem);

            var resultado = await service.CriarAsync(new CriarMetaEconomiaDto
            {
                Nome = "Reserva",
                ValorAlvo = 500m,
                IdConta = contaDeOutroUsuario.IdConta
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Conta não encontrada");
            Assert.Equal(0, metas.Salvamentos);
        }

        // E1 — data-alvo no passado.
        [Fact]
        public async Task CriarAsync_com_data_alvo_no_passado_notifica_e_nao_grava()
        {
            var (service, metas, notificador) = Montar();

            var resultado = await service.CriarAsync(new CriarMetaEconomiaDto
            {
                Nome = "Reserva",
                ValorAlvo = 500m,
                DataAlvo = Fabrica.DiasAtras(1)
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("passado");
            Assert.Equal(0, metas.Salvamentos);
        }

        // ======================= UC12 — Registrar aporte em meta =======================

        [Fact]
        public async Task RegistrarAporteAsync_com_valor_valido_registra_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia(valorAlvo: 1000m);
            armazem.Semear(meta);

            var (service, metas, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto
            {
                Valor = 200m,
                Data = DateTime.UtcNow
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(200m, resultado!.Valor);
            Assert.Equal(meta.IdMeta, resultado.IdMeta);
            Assert.Equal(1, metas.Salvamentos);
            Assert.Equal(200m, meta.TotalAportado());
            Assert.False(meta.Concluida);
        }

        [Fact]
        public async Task RegistrarAporteAsync_quando_total_atinge_o_valor_alvo_marca_concluida()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia(valorAlvo: 500m);
            armazem.Semear(meta);

            var (service, _, notificador) = Montar(armazem);

            await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto { Valor = 500m, Data = DateTime.UtcNow });

            notificador.NaoDeveNotificar();
            Assert.True(meta.Concluida);
        }

        [Fact]
        public async Task RegistrarAporteAsync_acumula_totais_de_varios_aportes()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia(valorAlvo: 1000m);
            armazem.Semear(meta);

            var (service, _, notificador) = Montar(armazem);

            await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto { Valor = 300m, Data = DateTime.UtcNow });
            await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto { Valor = 400m, Data = DateTime.UtcNow });

            notificador.NaoDeveNotificar();
            Assert.Equal(700m, meta.TotalAportado());
            Assert.False(meta.Concluida);
        }

        [Fact]
        public async Task RegistrarAporteAsync_com_meta_inexistente_notifica_e_nao_registra()
        {
            var (service, metas, notificador) = Montar();

            var resultado = await service.RegistrarAporteAsync(999, new RegistrarAporteMetaDto { Valor = 100m, Data = DateTime.UtcNow });

            Assert.Null(resultado);
            notificador.DeveNotificar("não encontrada");
            Assert.Equal(0, metas.Salvamentos);
        }

        [Fact]
        public async Task RegistrarAporteAsync_com_meta_de_outro_usuario_notifica_e_nao_registra()
        {
            var armazem = new ArmazemFake();
            var metaDeOutroUsuario = Fabrica.MetaEconomia(idUsuario: "outro-usuario");
            armazem.Semear(metaDeOutroUsuario);

            var (service, metas, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAporteAsync(metaDeOutroUsuario.IdMeta, new RegistrarAporteMetaDto { Valor = 100m, Data = DateTime.UtcNow });

            Assert.Null(resultado);
            notificador.DeveNotificar("não encontrada");
            Assert.Equal(0, metas.Salvamentos);
        }

        [Fact]
        public async Task RegistrarAporteAsync_com_transacao_inexistente_notifica_e_nao_registra()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia();
            armazem.Semear(meta);

            var (service, metas, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto
            {
                Valor = 100m,
                Data = DateTime.UtcNow,
                IdTransacao = 999
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("Transação não encontrada");
            Assert.Equal(0, metas.Salvamentos);
            Assert.Equal(0m, meta.TotalAportado());
        }

        [Fact]
        public async Task RegistrarAporteAsync_com_transacao_existente_do_usuario_registra()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia();
            var transacao = Fabrica.Receita();
            armazem.Semear(meta);
            armazem.Semear(transacao);

            var (service, _, notificador) = Montar(armazem);

            var resultado = await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto
            {
                Valor = 100m,
                Data = DateTime.UtcNow,
                IdTransacao = transacao.IdTransacao
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(transacao.IdTransacao, resultado!.IdTransacao);
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_metas_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            armazem.Semear(
                Fabrica.MetaEconomia(nome: "Minha meta"),
                Fabrica.MetaEconomia(idUsuario: "outro-usuario", nome: "Meta de outro"));

            var (service, _, _) = Montar(armazem);

            var metas = await service.ObterTodosAsync();

            Assert.Single(metas);
            Assert.Equal("Minha meta", metas.Single().Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_meta_existente_retorna_dto_com_total_aportado()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia(valorAlvo: 300m);
            meta.RegistrarAporte(120m, DateTime.UtcNow, null);
            armazem.Semear(meta);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(meta.IdMeta);

            Assert.NotNull(resultado);
            Assert.Equal(120m, resultado!.TotalAportado);
        }

        [Fact]
        public async Task ObterPorIdAsync_meta_inexistente_retorna_null()
        {
            var (service, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_de_meta_de_outro_usuario_retorna_null()
        {
            var armazem = new ArmazemFake();
            var metaDeOutroUsuario = Fabrica.MetaEconomia(idUsuario: "outro-usuario");
            armazem.Semear(metaDeOutroUsuario);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(metaDeOutroUsuario.IdMeta);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterAportesAsync_retorna_aportes_registrados_na_meta()
        {
            var armazem = new ArmazemFake();
            var meta = Fabrica.MetaEconomia();
            armazem.Semear(meta);

            var (service, _, _) = Montar(armazem);

            await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto { Valor = 50m, Data = DateTime.UtcNow });
            await service.RegistrarAporteAsync(meta.IdMeta, new RegistrarAporteMetaDto { Valor = 70m, Data = DateTime.UtcNow });

            var aportes = await service.ObterAportesAsync(meta.IdMeta);

            Assert.Equal(2, aportes.Count);
            Assert.Equal(120m, aportes.Sum(a => a.Valor));
        }
    }
}
