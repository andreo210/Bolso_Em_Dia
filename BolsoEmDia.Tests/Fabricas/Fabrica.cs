using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Tests.Fakes;

namespace BolsoEmDia.Tests.Fabricas
{
    /// <summary>
    /// Entidades válidas para os testes, com valores padrão que passam nas validações de
    /// <c>Criar</c>. O teste só informa o que é relevante para o caso que está verificando —
    /// assim, quando uma validação nova entra na entidade, corrige-se um lugar só.
    /// </summary>
    public static class Fabrica
    {
        /// <summary>
        /// Usuário "logado" nos testes de serviço — mesmo valor que <see cref="UsuarioFake.IdPadrao"/>.
        /// Entidades semeadas com este id são as que <c>IContaService</c>/<c>ITransacaoService</c>
        /// enxergam quando montados com <c>new UsuarioFake()</c> sem argumento.
        /// </summary>
        public const string IdUsuarioPadrao = UsuarioFake.IdPadrao;

        /// <summary>Data segura para "futuro": as entidades validam contra <c>DateTime.UtcNow</c>.</summary>
        public static DateTime DaquiADias(int dias) => DateTime.UtcNow.AddDays(dias);

        /// <summary>Data segura para "passado". Nunca use literal: ele envelhece e o teste quebra sozinho.</summary>
        public static DateTime DiasAtras(int dias) => DateTime.UtcNow.AddDays(-dias);

        /// <summary>
        /// Escreve na chave primária mesmo com set privado. Em teste isso é necessário porque o id
        /// normalmente viria do banco, e sem ele todo filtro por id casaria com a entidade errada.
        /// Só é preciso chamar quando o teste precisa de um id específico — <c>ArmazemFake.Semear</c>
        /// já atribui id a quem entra sem nenhum.
        /// </summary>
        public static void DefinirId(object entidade, int id) => ChavePrimaria.Definir(entidade, id);

        public static Conta Conta(
            string idUsuario = IdUsuarioPadrao,
            string nome = "Conta corrente",
            TipoConta tipo = TipoConta.Corrente,
            decimal saldoInicial = 0m,
            bool ativa = true)
        {
            var conta = Domain.Entidades.Conta.Criar(idUsuario, nome, tipo, saldoInicial);

            if (!ativa) conta.Desativar();

            return conta;
        }

        public static Categoria Categoria(
            string idUsuario = IdUsuarioPadrao,
            string nome = "Salário",
            TipoCategoria tipo = TipoCategoria.Receita,
            int? idCategoriaPai = null,
            bool ativa = true)
        {
            var categoria = Domain.Entidades.Categoria.Criar(idUsuario, nome, tipo, idCategoriaPai);

            if (!ativa) categoria.Desativar();

            return categoria;
        }

        public static Transacao Receita(
            string idUsuario = IdUsuarioPadrao,
            int idConta = 1,
            int idCategoria = 1,
            DateTime? data = null,
            decimal valor = 100m,
            string? descricao = "Receita de teste")
            => Domain.Entidades.Transacao.RegistrarReceita(idUsuario, idConta, idCategoria, data ?? DateTime.UtcNow, valor, descricao);

        public static Transacao Despesa(
            string idUsuario = IdUsuarioPadrao,
            int idConta = 1,
            int idCategoria = 1,
            DateTime? data = null,
            decimal valor = 100m,
            string? descricao = "Despesa de teste")
            => Domain.Entidades.Transacao.RegistrarDespesa(idUsuario, idConta, idCategoria, data ?? DateTime.UtcNow, valor, descricao);
    }
}
