namespace BolsoEmDia.Front.Models.Request.Autenticacao
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
    }
}
