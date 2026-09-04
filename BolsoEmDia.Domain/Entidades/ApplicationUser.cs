using Microsoft.AspNetCore.Identity;

namespace BolsoEmDia.Domain.Entidades
{
    /// <summary>
    /// Usuário do sistema. Herda o Identity (hash de senha, e-mail, etc.) — exceção deliberada
    /// à regra de "entidade rica com Criar()": criação e autenticação são responsabilidade do
    /// UserManager/SignInManager, não do domínio.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = null!;
    }
}
