using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Domain.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BolsoEmDia.Application.Services.AutenticacaoServices
{
    public class AutenticacaoService : IAutenticacaoService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly INotificadorService _notificador;
        private readonly RsaKeyService _rsaKeyService;

        public AutenticacaoService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            INotificadorService notificador,
            RsaKeyService rsaKeyService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _notificador = notificador;
            _rsaKeyService = rsaKeyService;
        }

        public async Task<TokenDto?> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken ct = default)
        {
            var usuario = new ApplicationUser
            {
                Nome = dto.Nome,
                Email = dto.Email,
                UserName = dto.Email
            };

            var resultado = await _userManager.CreateAsync(usuario, dto.Senha);
            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                    _notificador.Add(erro.Description);

                return null;
            }

            return GerarToken(usuario);
        }

        public async Task<TokenDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario is null)
            {
                _notificador.Add("E-mail ou senha inválidos");
                return null;
            }

            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Senha, lockoutOnFailure: true);
            if (!resultado.Succeeded)
            {
                _notificador.Add(resultado.IsLockedOut
                    ? "Usuário temporariamente bloqueado por tentativas inválidas"
                    : "E-mail ou senha inválidos");
                return null;
            }

            return GerarToken(usuario);
        }

        // O CurrentUser (Infra) lê o usuário logado via ClaimTypes.NameIdentifier — precisa entrar
        // explicitamente aqui porque o JwtBearer, a partir do .NET 8, não mapeia mais "sub" para
        // esse claim por padrão.
        private TokenDto GerarToken(ApplicationUser usuario)
        {
            var horasExpiracao = double.Parse(_config["Jwt:ExpiresHours"] ?? "8");
            var expiraEm = DateTime.UtcNow.AddHours(horasExpiracao);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiraEm,
                signingCredentials: _rsaKeyService.GetSigningCredentials());

            return new TokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiraEm = expiraEm,
                Nome = usuario.Nome,
                Email = usuario.Email!
            };
        }
    }
}
