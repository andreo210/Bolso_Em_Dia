using System.Security.Claims;
using BolsoEmDia.Front.Models.Request.Autenticacao;
using BolsoEmDia.Front.Models.Response.Autenticacao;
using BolsoEmDia.Front.Models.Validadores;
using BolsoEmDia.Front.Services.Servicos.Autenticacao;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BolsoEmDia.Front.Extensions
{
    /// <summary>
    /// Login/registro/logout são endpoints HTTP simples (não componentes Blazor interativos)
    /// de propósito: gravar o cookie de autenticação exige escrever o cabeçalho da resposta
    /// antes dela terminar, e isso não é possível depois que o circuito SignalR assume a página.
    /// Ficam sob /auth/... (e não /login, /registrar) porque MapRazorComponents já registra
    /// esses caminhos para todo verbo HTTP — mapear o mesmo caminho de novo dá AmbiguousMatchException.
    /// </summary>
    public static class AutenticacaoEndpointsExtensions
    {
        public static void MapAutenticacaoEndpoints(this WebApplication app)
        {
            app.MapPost("/auth/login", async (
                HttpContext http,
                IAntiforgery antiforgery,
                IAutenticacaoService autenticacaoService) =>
            {
                if (!await TokenValido(http, antiforgery))
                    return Results.BadRequest("Token de segurança inválido. Recarregue a página e tente de novo.");

                var form = await http.Request.ReadFormAsync();
                var returnUrl = form["returnUrl"].ToString();
                var request = new LoginRequest { Email = form["email"].ToString(), Senha = form["senha"].ToString() };

                if (!new LoginRequestValidator().Validate(request).IsValid)
                    return RedirecionarComErro("/login", "Informe e-mail e senha válidos", returnUrl);

                var resultado = await autenticacaoService.LoginAsync(request, http.RequestAborted);

                if (resultado is null)
                    return RedirecionarComErro("/login", "E-mail ou senha inválidos", returnUrl);

                await AssinarCookieAsync(http, resultado);

                return Results.LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
            })
            .AllowAnonymous()
            .DisableAntiforgery();

            app.MapPost("/auth/registrar", async (
                HttpContext http,
                IAntiforgery antiforgery,
                IAutenticacaoService autenticacaoService) =>
            {
                if (!await TokenValido(http, antiforgery))
                    return Results.BadRequest("Token de segurança inválido. Recarregue a página e tente de novo.");

                var form = await http.Request.ReadFormAsync();
                var request = new RegistrarRequest
                {
                    Nome = form["nome"].ToString(),
                    Email = form["email"].ToString(),
                    Senha = form["senha"].ToString()
                };

                var validacao = new RegistrarRequestValidator().Validate(request);

                if (!validacao.IsValid)
                    return RedirecionarComErro("/registrar", string.Join("; ", validacao.Errors.Select(e => e.ErrorMessage)), null);

                var criado = await autenticacaoService.RegistrarAsync(request, http.RequestAborted);

                if (!criado)
                    return RedirecionarComErro("/registrar", "Não foi possível concluir o cadastro. Verifique os dados e tente novamente.", null);

                // A Api não devolve token no registro: loga em seguida com as mesmas credenciais
                // para não obrigar o usuário a digitar tudo de novo na tela de login.
                var login = await autenticacaoService.LoginAsync(new LoginRequest { Email = request.Email, Senha = request.Senha }, http.RequestAborted);

                if (login is null)
                    return RedirecionarComErro("/login", "Cadastro criado. Faça login para continuar.", null);

                await AssinarCookieAsync(http, login);

                return Results.LocalRedirect("/");
            })
            .AllowAnonymous()
            .DisableAntiforgery();

            app.MapPost("/auth/logout", async (HttpContext http, IAntiforgery antiforgery) =>
            {
                if (!await TokenValido(http, antiforgery))
                    return Results.BadRequest();

                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.LocalRedirect("/login");
            })
            .DisableAntiforgery();
        }

        private static async Task<bool> TokenValido(HttpContext http, IAntiforgery antiforgery)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(http);
                return true;
            }
            catch (AntiforgeryValidationException)
            {
                return false;
            }
        }

        private static IResult RedirecionarComErro(string caminho, string mensagem, string? returnUrl)
        {
            var url = $"{caminho}?erro={Uri.EscapeDataString(mensagem)}";

            if (!string.IsNullOrWhiteSpace(returnUrl))
                url += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

            return Results.LocalRedirect(url);
        }

        private static async Task AssinarCookieAsync(HttpContext http, LoginResponse resultado)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, resultado.Nome),
                new Claim(ClaimTypes.Email, resultado.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = new DateTimeOffset(DateTime.SpecifyKind(resultado.ExpiraEm, DateTimeKind.Utc))
            };
            properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = resultado.AccessToken }
            });

            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }
    }
}
