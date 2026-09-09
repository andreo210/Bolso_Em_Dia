using System.ComponentModel.DataAnnotations;

namespace BolsoEmDia.Application.Models.Dto
{
    public class RegistrarUsuarioDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = null!;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "E-mail é obrigatório")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = null!;
    }

    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiraEm { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
