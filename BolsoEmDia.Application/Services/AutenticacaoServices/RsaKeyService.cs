using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace BolsoEmDia.Application.Services.AutenticacaoServices
{
    /// <summary>
    /// Par de chaves RSA usado para assinar (RS256) e validar os JWTs emitidos pela própria Api.
    /// A chave privada nunca sai do processo; só a pública é publicada em GET /.well-known/jwks.json.
    /// Persistida em disco (Jwt:PrivateKeyPath) para sobreviver a restarts — sem isso, todo token
    /// emitido antes de reiniciar a Api vira inválido.
    /// </summary>
    public class RsaKeyService
    {
        private readonly RSA _rsa;

        public string KeyId { get; }
        public RsaSecurityKey Key { get; }

        public RsaKeyService(IConfiguration config)
        {
            var keyPath = config["Jwt:PrivateKeyPath"]
                ?? throw new InvalidOperationException("Jwt:PrivateKeyPath não configurado.");
            KeyId = config["Jwt:KeyId"] ?? "bolso-em-dia-key-01";

            if (!File.Exists(keyPath))
                CriarNovaChave(keyPath);

            _rsa = RSA.Create();
            _rsa.ImportFromPem(File.ReadAllText(keyPath));
            Key = new RsaSecurityKey(_rsa) { KeyId = KeyId };
        }

        private static void CriarNovaChave(string path)
        {
            var diretorio = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(diretorio)) Directory.CreateDirectory(diretorio);

            using var rsa = RSA.Create(2048);
            File.WriteAllText(path, rsa.ExportRSAPrivateKeyPem());
        }

        public SigningCredentials GetSigningCredentials()
            => new(Key, SecurityAlgorithms.RsaSha256);

        public JsonWebKey GetPublicJwk()
        {
            var parametros = _rsa.ExportParameters(includePrivateParameters: false);

            return new JsonWebKey
            {
                Kty = "RSA",
                Use = "sig",
                Kid = KeyId,
                Alg = "RS256",
                N = Base64UrlEncoder.Encode(parametros.Modulus),
                E = Base64UrlEncoder.Encode(parametros.Exponent)
            };
        }
    }
}
