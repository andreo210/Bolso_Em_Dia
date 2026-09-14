namespace BolsoEmDia.Front.Models.Request.Autenticacao
{
    public class RegistrarRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
    }
}
