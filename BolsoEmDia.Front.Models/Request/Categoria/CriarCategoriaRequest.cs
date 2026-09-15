using BolsoEmDia.Front.Models.Enum;

namespace BolsoEmDia.Front.Models.Request.Categoria
{
    public class CriarCategoriaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public TipoCategoria Tipo { get; set; }
        public int? IdCategoriaPai { get; set; }
        public string Cor { get; set; } = "#6c757d";
        public string Icone { get; set; } = "bi-tag";
    }
}
