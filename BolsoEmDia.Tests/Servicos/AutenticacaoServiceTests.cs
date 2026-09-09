using BolsoEmDia.Application.Configuration.Utils.NotificadorServices;
using BolsoEmDia.Application.Models.Dto;
using BolsoEmDia.Application.Services.AutenticacaoServices;
using BolsoEmDia.Tests.Assercoes;
using BolsoEmDia.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BolsoEmDia.Tests.Servicos
{
    /// <summary>
    /// Gera uma chave RSA de teste uma única vez por classe (via <see cref="IClassFixture{T}"/>) —
    /// gerar RSA 2048 bits é razoavelmente caro para repetir em cada um dos testes, e a chave em si
    /// não é estado mutável relevante ao que os testes verificam. Some o arquivo temporário ao final.
    /// </summary>
    public sealed class AutenticacaoFixture : IDisposable
    {
        private readonly string _caminhoChave =
            Path.Combine(Path.GetTempPath(), $"bolsoemdia-tests-{Guid.NewGuid():N}.pem");

        public IConfiguration Config { get; }
        public RsaKeyService RsaKeyService { get; }

        public AutenticacaoFixture()
        {
            Config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:PrivateKeyPath"] = _caminhoChave,
                ["Jwt:Issuer"] = "bolsoemdia-tests",
                ["Jwt:Audience"] = "bolsoemdia-tests",
                ["Jwt:ExpiresHours"] = "8"
            }).Build();

            RsaKeyService = new RsaKeyService(Config);
        }

        public void Dispose()
        {
            if (File.Exists(_caminhoChave)) File.Delete(_caminhoChave);
        }
    }

    public class AutenticacaoServiceTests : IClassFixture<AutenticacaoFixture>
    {
        private readonly AutenticacaoFixture _fixture;

        public AutenticacaoServiceTests(AutenticacaoFixture fixture) => _fixture = fixture;

        private (AutenticacaoService Service, UserManagerFake UserManager, NotificadorService Notificador) Montar()
        {
            var userManager = new UserManagerFake();
            var signInManager = new SignInManagerFake(userManager);
            var notificador = new NotificadorService();

            var service = new AutenticacaoService(userManager, signInManager, _fixture.Config, notificador, _fixture.RsaKeyService);

            return (service, userManager, notificador);
        }

        [Fact]
        public async Task RegistrarAsync_com_email_novo_cria_usuario_e_devolve_token()
        {
            var (service, _, notificador) = Montar();

            var resultado = await service.RegistrarAsync(new RegistrarUsuarioDto
            {
                Nome = "Ana Souza",
                Email = "ana@teste.com",
                Senha = "Senha@123"
            });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Ana Souza", resultado!.Nome);
            Assert.Equal("ana@teste.com", resultado.Email);
            Assert.False(string.IsNullOrWhiteSpace(resultado.AccessToken));
            Assert.True(resultado.ExpiraEm > DateTime.UtcNow);
        }

        [Fact]
        public async Task RegistrarAsync_com_email_ja_cadastrado_notifica_e_nao_devolve_token()
        {
            var (service, userManager, notificador) = Montar();
            userManager.SemearUsuario("ana@teste.com", "Senha@123");

            var resultado = await service.RegistrarAsync(new RegistrarUsuarioDto
            {
                Nome = "Ana Souza",
                Email = "ana@teste.com",
                Senha = "OutraSenha@123"
            });

            Assert.Null(resultado);
            notificador.DeveNotificar("uso");
        }

        [Fact]
        public async Task LoginAsync_com_email_inexistente_notifica_mensagem_generica()
        {
            var (service, _, notificador) = Montar();

            var resultado = await service.LoginAsync(new LoginDto { Email = "naoexiste@teste.com", Senha = "qualquer" });

            Assert.Null(resultado);
            notificador.DeveNotificar("inválidos");
        }

        // A mesma mensagem de "e-mail não existe" vale para "senha errada": não dar a distinguir
        // qual dos dois é o motivo é o que impede descobrir e-mails cadastrados por tentativa e erro.
        [Fact]
        public async Task LoginAsync_com_senha_errada_notifica_a_mesma_mensagem_generica_do_email_inexistente()
        {
            var (service, userManager, notificador) = Montar();
            userManager.SemearUsuario("ana@teste.com", "Senha@123");

            var resultado = await service.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "senhaErrada" });

            Assert.Null(resultado);
            notificador.DeveNotificar("inválidos");
        }

        [Fact]
        public async Task LoginAsync_com_usuario_bloqueado_notifica_mensagem_especifica_de_bloqueio()
        {
            var (service, userManager, notificador) = Montar();
            userManager.SemearUsuario("ana@teste.com", "Senha@123", bloqueado: true);

            var resultado = await service.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "Senha@123" });

            Assert.Null(resultado);
            notificador.DeveNotificar("bloqueado");
        }

        [Fact]
        public async Task LoginAsync_com_credenciais_corretas_devolve_token()
        {
            var (service, userManager, notificador) = Montar();
            userManager.SemearUsuario("ana@teste.com", "Senha@123", nome: "Ana Souza");

            var resultado = await service.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "Senha@123" });

            notificador.NaoDeveNotificar();
            Assert.NotNull(resultado);
            Assert.Equal("Ana Souza", resultado!.Nome);
            Assert.Equal("ana@teste.com", resultado.Email);
            Assert.False(string.IsNullOrWhiteSpace(resultado.AccessToken));
        }
    }
}
