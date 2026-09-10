using BolsoEmDia.Domain.Entidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BolsoEmDia.Tests.Fakes
{
    /// <summary>
    /// <c>UserManager&lt;TUser&gt;</c> e <c>SignInManager&lt;TUser&gt;</c> são classes concretas do
    /// ASP.NET Identity, não interfaces — não dá para trocar por um <c>RepositorioFake</c>. O caminho
    /// sem biblioteca de mock é subclassear e sobrescrever só os dois métodos que
    /// <c>AutenticacaoService</c> chama (<c>CreateAsync</c>/<c>FindByEmailAsync</c> e
    /// <c>CheckPasswordSignInAsync</c>, todos <c>virtual</c>); o resto das dependências do
    /// construtor nunca é exercitado, então recebe um stub mínimo.
    /// </summary>
    internal sealed class UserStoreNaoUsado : IUserStore<ApplicationUser>
    {
        public void Dispose() { }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) => throw new NotImplementedException();
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) => throw new NotImplementedException();
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => throw new NotImplementedException();
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => throw new NotImplementedException();
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => throw new NotImplementedException();
    }

    /// <summary>
    /// "Banco" de usuários em memória, expondo <see cref="SemearUsuario"/> para os testes de
    /// <c>LoginAsync</c> e sobrescrevendo <see cref="CreateAsync"/>/<see cref="FindByEmailAsync"/>
    /// — os dois únicos métodos que <c>AutenticacaoService</c> usa do UserManager.
    /// </summary>
    public class UserManagerFake : UserManager<ApplicationUser>
    {
        private readonly List<(ApplicationUser Usuario, string Senha)> _usuarios = new();
        private readonly HashSet<string> _bloqueados = new(StringComparer.OrdinalIgnoreCase);

        public UserManagerFake() : base(
            new UserStoreNaoUsado(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            services: null!,
            logger: NullLogger<UserManager<ApplicationUser>>.Instance)
        {
        }

        /// <summary>Usuário já cadastrado, para os cenários de <c>LoginAsync</c>.</summary>
        public ApplicationUser SemearUsuario(string email, string senha, string nome = "Usuário de teste", bool bloqueado = false)
        {
            var usuario = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = email, UserName = email, Nome = nome };
            _usuarios.Add((usuario, senha));
            if (bloqueado) _bloqueados.Add(email);
            return usuario;
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            if (_usuarios.Any(u => string.Equals(u.Usuario.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = $"E-mail '{user.Email}' já está em uso."
                }));
            }

            _usuarios.Add((user, password));
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
            => Task.FromResult(_usuarios.FirstOrDefault(u => string.Equals(u.Usuario.Email, email, StringComparison.OrdinalIgnoreCase)).Usuario);

        internal bool SenhaConfere(ApplicationUser usuario, string senha)
            => _usuarios.Any(u => u.Usuario.Id == usuario.Id && u.Senha == senha);

        internal bool EstaBloqueado(ApplicationUser usuario)
            => usuario.Email != null && _bloqueados.Contains(usuario.Email);
    }

    /// <summary>Sobrescreve só a checagem de senha; o resto do SignInManager nunca é exercitado.</summary>
    public class SignInManagerFake : SignInManager<ApplicationUser>
    {
        private readonly UserManagerFake _userManager;

        public SignInManagerFake(UserManagerFake userManager) : base(
            userManager,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new UserClaimsPrincipalFactoryNaoUsado(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            new AuthenticationSchemeProviderNaoUsado(),
            new ConfirmacaoSempreOk())
        {
            _userManager = userManager;
        }

        public override Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {
            if (_userManager.EstaBloqueado(user))
                return Task.FromResult(SignInResult.LockedOut);

            return Task.FromResult(_userManager.SenhaConfere(user, password) ? SignInResult.Success : SignInResult.Failed);
        }

        private sealed class UserClaimsPrincipalFactoryNaoUsado : IUserClaimsPrincipalFactory<ApplicationUser>
        {
            public Task<System.Security.Claims.ClaimsPrincipal> CreateAsync(ApplicationUser user) => throw new NotImplementedException();
        }

        private sealed class ConfirmacaoSempreOk : IUserConfirmation<ApplicationUser>
        {
            public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user) => Task.FromResult(true);
        }

        private sealed class AuthenticationSchemeProviderNaoUsado : IAuthenticationSchemeProvider
        {
            public Task<AuthenticationScheme?> GetSchemeAsync(string name) => Task.FromResult<AuthenticationScheme?>(null);
            public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync() => Task.FromResult(Enumerable.Empty<AuthenticationScheme>());
            public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync() => Task.FromResult(Enumerable.Empty<AuthenticationScheme>());
            public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
            public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
            public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
            public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
            public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
            public void AddScheme(AuthenticationScheme scheme) { }
            public void RemoveScheme(string name) { }
        }
    }
}
