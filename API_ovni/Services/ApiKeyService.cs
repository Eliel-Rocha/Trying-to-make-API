// Nome do Arquivo: ApiKeyService.cs
using System.Security.Cryptography;
using System.Text;

// Esta classe é responsável por gerar chaves de API seguras e hasheá-las para armazenamento seguro no banco de dados. Ela é utilizada tanto no processo de geração de chaves (cadastro) quanto na validação de chaves (login) para garantir que as chaves sejam únicas, seguras e não possam ser revertidas a partir do hash armazenado.
namespace API_ovni.Services
{    public class ApiKeyService
    {
        public string GenerateSecureApiKey(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
            }
        }

        public string HashApiKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(apiKey);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}