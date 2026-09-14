namespace BolsoEmDia.Front.Models.Response.Autenticacao
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiraEm { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
