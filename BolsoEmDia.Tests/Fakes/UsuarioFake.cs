using BolsoEmDia.Infra.Data.CurrentUsers;

namespace BolsoEmDia.Tests.Fakes
{
    /// <summary>
    /// <see cref="ICurrentUser"/> fixo para teste de serviço — o real depende de <c>HttpContext</c>,
    /// que não existe fora de uma requisição. <see cref="IdPadrao"/> é o id que <c>Fabrica</c> usa
    /// por padrão em toda entidade com dono, então <c>new UsuarioFake()</c> "loga" como o mesmo
    /// usuário que semeou os dados — é o que faz <c>ContaService</c>/<c>TransacaoService</c>
    /// enxergarem o que o teste semeou.
    /// </summary>
    public class UsuarioFake : ICurrentUser
    {
        public const string IdPadrao = "usuario-teste-1";

        public UsuarioFake(string? userId = IdPadrao) => UserId = userId;

        public string? UserId { get; }

        public bool IsAuthenticated => UserId != null;
    }
}
