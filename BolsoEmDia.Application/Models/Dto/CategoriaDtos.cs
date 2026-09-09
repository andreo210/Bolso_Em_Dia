using System.ComponentModel.DataAnnotations;
using BolsoEmDia.Domain.Entidades;

namespace BolsoEmDia.Application.Models.Dto
{
    public class CategoriaDto
    {
        public int IdCategoria { get; set; }
        public string Nome { get; set; } = null!;
        public TipoCategoria Tipo { get; set; }
        public int? IdCategoriaPai { get; set; }
        public bool Ativa { get; set; }
    }

    public class CriarCategoriaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Tipo é obrigatório")]
        public TipoCategoria Tipo { get; set; }

        public int? IdCategoriaPai { get; set; }
    }
}
