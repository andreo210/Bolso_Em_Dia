using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Response.Categoria
{
    public class CategoriaResponse
    {
        public int IdCategoria { get; set; }
        public string Nome { get; set; } = null!;
        public TipoCategoria Tipo { get; set; }
        public int? IdCategoriaPai { get; set; }
        public bool Ativa { get; set; }
        public string Cor { get; set; } = null!;
        public string Icone { get; set; } = null!;
    }
}
