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

        public DateTime DataCriacao { get; set; }
        public string? IdUsuarioCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public string? IdUsuarioModificacao { get; set; }

        protected Categoria() { } // EF

        public static Categoria Criar(string idUsuario, string nome, TipoCategoria tipo, int? idCategoriaPai = null)
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
                Ativa = true
            };
        }

        public void Renomear(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome é obrigatório");

            Nome = nome;
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
