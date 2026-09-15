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
        public string Cor { get; set; } = null!;
        public string Icone { get; set; } = null!;
    }

    public class CriarCategoriaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Tipo é obrigatório")]
        public TipoCategoria Tipo { get; set; }

        public int? IdCategoriaPai { get; set; }

        // Opcionais: Categoria.Criar aplica CorPadrao/IconePadrao quando vierem vazios.
        [MaxLength(9)]
        public string? Cor { get; set; }

        [MaxLength(50)]
        public string? Icone { get; set; }
    }

    public class AtualizarCategoriaDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Cor é obrigatória")]
        [MaxLength(9)]
        public string Cor { get; set; } = null!;

        [Required(ErrorMessage = "Ícone é obrigatório")]
        [MaxLength(50)]
        public string Icone { get; set; } = null!;
    }
}
