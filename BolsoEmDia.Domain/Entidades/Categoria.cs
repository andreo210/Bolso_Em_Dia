using BolsoEmDia.Domain.Auditoria;

namespace BolsoEmDia.Domain.Entidades
{
    public class Categoria : IPertenceAoUsuario, IAuditoria
    {
        public int IdCategoria { get; private set; }
        public string IdUsuario { get; private set; } = null!;
        public string Nome { get; private set; } = null!;
        public TipoCategoria Tipo { get; private set; }
        public int? IdCategoriaPai { get; private set; }
        public bool Ativa { get; private set; }
        public string Cor { get; private set; } = null!;
        public string Icone { get; private set; } = null!;

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        // Aplicados quando o cadastro não escolhe cor/ícone — mesmo cinza neutro e mesmo
        // ícone genérico (bi-tag) usados em todo o front como "sem identidade visual definida".
        public const string CorPadrao = "#6c757d";
        public const string IconePadrao = "bi-tag";

        protected Categoria() { } // EF

        public static Categoria Criar(
            string idUsuario, string nome, TipoCategoria tipo, int? idCategoriaPai = null,
            string? cor = null, string? icone = null)
        {
            if (string.IsNullOrWhiteSpace(idUsuario))
                throw new DomainException("Usuário é obrigatório");
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");

            return new Categoria
            {
                IdUsuario = idUsuario,
                Nome = nome,
                Tipo = tipo,
                IdCategoriaPai = idCategoriaPai,
                Cor = string.IsNullOrWhiteSpace(cor) ? CorPadrao : cor,
                Icone = string.IsNullOrWhiteSpace(icone) ? IconePadrao : icone,
                Ativa = true
            };
        }

        public void Renomear(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");

            Nome = nome;
        }

        public void AlterarAparencia(string cor, string icone)
        {
            if (string.IsNullOrWhiteSpace(cor))
                throw new DomainException("Cor é obrigatória");
            if (string.IsNullOrWhiteSpace(icone))
                throw new DomainException("Ícone é obrigatório");

            Cor = cor;
            Icone = icone;
        }

        public void Ativar() => Ativa = true;

        public void Desativar() => Ativa = false;
    }

    public enum TipoCategoria
    {
        Receita = 0,
        Despesa = 1
    }
}
