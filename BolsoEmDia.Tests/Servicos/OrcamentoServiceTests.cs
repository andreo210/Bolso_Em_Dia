using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.OrcamentoServices;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fabricas;
using BolsoEmDia.Tests.Fakes;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    public class OrcamentoServiceTests
    {
        private static (OrcamentoService Service, OrcamentoRepositoryFake Orcamentos, NotificadorService Notificador)
            Montar(ArmazemFake? armazem = null, string? idUsuario = UsuarioFake.IdPadrao)
        {
            var arm = armazem ?? new ArmazemFake();
            var orcamentos = new OrcamentoRepositoryFake(arm);
            var categorias = new CategoriaRepositoryFake(arm);
            var transacoes = new TransacaoRepositoryFake(arm);
            var notificador = new NotificadorService();

            var service = new OrcamentoService(orcamentos, categorias, transacoes, new UsuarioFake(idUsuario), notificador);

            return (service, orcamentos, notificador);
        }

        // ======================= UC08 — Definir orçamento mensal =======================

        [Fact]
        public async Task DefinirAsync_com_categoria_de_despesa_e_sem_orcamento_existente_cria_e_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, orcamentos, notificador) = Montar(armazem);

            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = categoria.IdCategoria,
                MesReferencia = Fabrica.MesAtual(),
                ValorMeta = 500m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(categoria.IdCategoria, resultado!.IdCategoria);
            Assert.Equal(500m, resultado.ValorMeta);
            Assert.Equal(Fabrica.MesAtual(), resultado.MesReferencia);
            Assert.Equal(1, orcamentos.Salvamentos);
        }

        [Fact]
        public async Task DefinirAsync_com_categoria_inexistente_notifica_e_nao_grava()
        {
            var (service, orcamentos, notificador) = Montar();

            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = 999,
                MesReferencia = Fabrica.MesAtual(),
                ValorMeta = 500m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("encontrada");
            Assert.Equal(0, orcamentos.Salvamentos);
        }

        [Fact]
        public async Task DefinirAsync_com_categoria_de_outro_usuario_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var categoriaDeOutroUsuario = Fabrica.Categoria(idUsuario: "outro-usuario", tipo: TipoCategoria.Despesa);
            armazem.Semear(categoriaDeOutroUsuario);

            var (service, orcamentos, notificador) = Montar(armazem);

            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = categoriaDeOutroUsuario.IdCategoria,
                MesReferencia = Fabrica.MesAtual(),
                ValorMeta = 500m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("encontrada");
            Assert.Equal(0, orcamentos.Salvamentos);
        }

        // E1 — categoria de receita não tem orçamento.
        [Fact]
        public async Task DefinirAsync_com_categoria_de_receita_notifica_e_nao_grava()
        {
            var armazem = new ArmazemFake();
            var categoriaDeReceita = Fabrica.Categoria(tipo: TipoCategoria.Receita);
            armazem.Semear(categoriaDeReceita);

            var (service, orcamentos, notificador) = Montar(armazem);

            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = categoriaDeReceita.IdCategoria,
                MesReferencia = Fabrica.MesAtual(),
                ValorMeta = 500m
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("receita");
            Assert.Equal(0, orcamentos.Salvamentos);
        }

        [Fact]
        public async Task DefinirAsync_quando_ja_existe_orcamento_da_categoria_no_mes_altera_meta_em_vez_de_duplicar()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);
            var orcamentoExistente = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 500m);
            armazem.Semear(orcamentoExistente);

            var (service, orcamentos, notificador) = Montar(armazem);

            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = categoria.IdCategoria,
                MesReferencia = Fabrica.MesAtual(),
                ValorMeta = 800m
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal(800m, resultado!.ValorMeta);
            Assert.Equal(1, orcamentos.Salvamentos);
            Assert.Single(orcamentos.Armazem.Tabela<Orcamento>());
        }

        [Fact]
        public async Task DefinirAsync_normaliza_mes_referencia_para_o_primeiro_dia_do_mes()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, _, notificador) = Montar(armazem);

            var mesAtual = Fabrica.MesAtual();
            var resultado = await service.DefinirAsync(new DefinirOrcamentoDto
            {
                IdCategoria = categoria.IdCategoria,
                MesReferencia = new DateOnly(mesAtual.Year, mesAtual.Month, 15),
                ValorMeta = 500m
            });

            notificador.NaoDeveNotificar();
            Assert.Equal(mesAtual, resultado!.MesReferencia);
        }

        // ======================= UC09 — Acompanhar progresso do orçamento =======================

        [Fact]
        public async Task ObterProgressoAsync_sem_orcamento_definido_retorna_null()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, Fabrica.MesAtual());

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterProgressoAsync_soma_despesas_da_categoria_no_mes_e_calcula_percentual()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 500m);
            armazem.Semear(orcamento);
            armazem.Semear(
                Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 100m, data: DateTime.UtcNow),
                Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 150m, data: DateTime.UtcNow));

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, Fabrica.MesAtual());

            Assert.NotNull(resultado);
            Assert.Equal(500m, resultado!.ValorMeta);
            Assert.Equal(250m, resultado.TotalGasto);
            Assert.Equal(0.5m, resultado.PercentualConsumido);
            Assert.False(resultado.Estourado);
        }

        [Fact]
        public async Task ObterProgressoAsync_quando_total_gasto_excede_a_meta_marca_estourado()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 100m);
            armazem.Semear(orcamento);
            armazem.Semear(Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 150m, data: DateTime.UtcNow));

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, Fabrica.MesAtual());

            Assert.NotNull(resultado);
            Assert.Equal(150m, resultado!.TotalGasto);
            Assert.True(resultado.Estourado);
        }

        // Regra de regras-negocio-financas: gasto lançado na subcategoria conta tanto para o total
        // dela quanto para o total da categoria-mãe — por isso o orçamento é definido só na mãe,
        // mas o progresso precisa somar as duas.
        [Fact]
        public async Task ObterProgressoAsync_inclui_despesas_de_subcategorias_no_total()
        {
            var armazem = new ArmazemFake();

            // Semear já antes de usar o Id: no ArmazemFake o Id só existe depois do Semear (como
            // no banco de verdade, que só gera o Id depois do INSERT) — usá-lo antes vincularia
            // tudo ao Id 0, e não ao Id real da categoria.
            var categoriaPai = Fabrica.Categoria(nome: "Alimentação", tipo: TipoCategoria.Despesa);
            armazem.Semear(categoriaPai);

            // Subcategoria de "Alimentação" — orçamento nunca é definido nela diretamente.
            var subcategoria = Fabrica.Categoria(nome: "Restaurante", tipo: TipoCategoria.Despesa, idCategoriaPai: categoriaPai.IdCategoria);
            armazem.Semear(subcategoria);

            // Orçamento de R$ 500 fica só na categoria-mãe.
            var orcamento = Fabrica.Orcamento(idCategoria: categoriaPai.IdCategoria, valorMeta: 500m);
            armazem.Semear(orcamento);

            // Uma despesa lançada direto na mãe (100) e outra lançada na subcategoria (50) — as
            // duas datadas de hoje, então nenhuma cai no filtro de "despesa futura".
            armazem.Semear(
                Fabrica.Despesa(idCategoria: categoriaPai.IdCategoria, valor: 100m, data: DateTime.UtcNow),
                Fabrica.Despesa(idCategoria: subcategoria.IdCategoria, valor: 50m, data: DateTime.UtcNow));

            var (service, _, _) = Montar(armazem);

            // Act — pergunta o progresso do orçamento da categoria-mãe no mês atual. Por dentro,
            // o serviço busca as subcategorias da categoria informada, soma as despesas de todas
            // elas (mãe + filhas) e só então divide pelo valor da meta.
            var resultado = await service.ObterProgressoAsync(categoriaPai.IdCategoria, Fabrica.MesAtual());

            Assert.NotNull(resultado);
            // 150 = 100 (mãe) + 50 (subcategoria). Se o serviço ignorasse a subcategoria, daria 100.
            Assert.Equal(150m, resultado!.TotalGasto);
        }

        [Fact]
        public async Task ObterProgressoAsync_ignora_despesas_de_outras_categorias()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(nome: "Alimentação", tipo: TipoCategoria.Despesa);
            var outraCategoria = Fabrica.Categoria(nome: "Transporte", tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria, outraCategoria);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 500m);
            armazem.Semear(orcamento);
            armazem.Semear(
                Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 100m, data: DateTime.UtcNow),
                Fabrica.Despesa(idCategoria: outraCategoria.IdCategoria, valor: 999m, data: DateTime.UtcNow));

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, Fabrica.MesAtual());

            Assert.NotNull(resultado);
            Assert.Equal(100m, resultado!.TotalGasto);
        }

        [Fact]
        public async Task ObterProgressoAsync_ignora_despesas_de_outro_mes()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, valorMeta: 500m);
            armazem.Semear(orcamento);
            armazem.Semear(
                Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 50m, data: DateTime.UtcNow),
                Fabrica.Despesa(idCategoria: categoria.IdCategoria, valor: 999m, data: Fabrica.DiasAtras(90)));

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, Fabrica.MesAtual());

            Assert.NotNull(resultado);
            Assert.Equal(50m, resultado!.TotalGasto);
        }

        // Despesa agendada para um mês que ainda não chegou: mesmo estando "dentro" do mês de
        // referência, nenhum dia dele já passou de "hoje" — por isso o total tem que ficar zerado.
        [Fact]
        public async Task ObterProgressoAsync_ignora_despesas_com_data_futura()
        {
            var armazem = new ArmazemFake();
            var categoria = Fabrica.Categoria(tipo: TipoCategoria.Despesa);
            armazem.Semear(categoria);
            var mesSeguinte = Fabrica.MesAtual().AddMonths(1);
            var orcamento = Fabrica.Orcamento(idCategoria: categoria.IdCategoria, mesReferencia: mesSeguinte, valorMeta: 500m);
            armazem.Semear(orcamento);
            armazem.Semear(Fabrica.Despesa(
                idCategoria: categoria.IdCategoria,
                valor: 200m,
                data: mesSeguinte.ToDateTime(TimeOnly.MinValue)));

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterProgressoAsync(categoria.IdCategoria, mesSeguinte);

            Assert.NotNull(resultado);
            Assert.Equal(0m, resultado!.TotalGasto);
            Assert.False(resultado.Estourado);
        }

        // ======================= Consultas =======================

        [Fact]
        public async Task ObterTodosAsync_retorna_somente_orcamentos_do_usuario_atual()
        {
            var armazem = new ArmazemFake();
            armazem.Semear(
                Fabrica.Orcamento(idCategoria: 1),
                Fabrica.Orcamento(idUsuario: "outro-usuario", idCategoria: 2));

            var (service, _, _) = Montar(armazem);

            var orcamentos = await service.ObterTodosAsync();

            Assert.Single(orcamentos);
            Assert.Equal(1, orcamentos.Single().IdCategoria);
        }

        [Fact]
        public async Task ObterPorIdAsync_orcamento_existente_retorna_dto()
        {
            var armazem = new ArmazemFake();
            var orcamento = Fabrica.Orcamento(idCategoria: 5, valorMeta: 300m);
            armazem.Semear(orcamento);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(orcamento.IdOrcamento);

            Assert.NotNull(resultado);
            Assert.Equal(300m, resultado!.ValorMeta);
        }

        [Fact]
        public async Task ObterPorIdAsync_orcamento_inexistente_retorna_null()
        {
            var (service, _, _) = Montar();

            var resultado = await service.ObterPorIdAsync(999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_de_orcamento_de_outro_usuario_retorna_null()
        {
            var armazem = new ArmazemFake();
            var orcamentoDeOutroUsuario = Fabrica.Orcamento(idUsuario: "outro-usuario");
            armazem.Semear(orcamentoDeOutroUsuario);

            var (service, _, _) = Montar(armazem);

            var resultado = await service.ObterPorIdAsync(orcamentoDeOutroUsuario.IdOrcamento);

            Assert.Null(resultado);
        }
    }
}
