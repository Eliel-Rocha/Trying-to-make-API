using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API_ovni.Models;

//esta classe é o controller para operações públicas, como gerar chaves de API para usuários.
namespace API_ovni.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class KeyRequestController : ControllerBase // Endpoint público, sem [Authorize]
    {
        private readonly IMongoCollection<ApiKeyUser> _userCollection;

        public KeyRequestController(IMongoCollection<ApiKeyUser> userCollection)
        {
            _userCollection = userCollection;
        }

        /// <summary>
        /// (PÚBLICO) Gera uma nova chave de API (somente leitura)
        /// </summary>
        [HttpPost("GenerateKey")]
        public async Task<IActionResult> GenerateKey([FromBody] ApiKeyRequest request)
        {
            const int MaxRetries = 3;
            int attempt = 0;
            string newKey;

            var fullName = $"{request.FirstName} {request.LastName}";
            var email = request.Email;

            while (attempt < MaxRetries)
            {
                try
                {
                    newKey = GenerateSecureApiKey(); // 1. Gera a nova chave

                    var newUser = new ApiKeyUser
                    {
                        ApiKey = newKey, // Usa a chave como ID
                        Name = fullName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    };

                    // 2. Tenta inserir no banco
                    await _userCollection.InsertOneAsync(newUser);

                    // s Retorna a chave e sai do loop
                    return Ok(new { apiKey = newKey });
                }
                catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Code == 11000)
                {
                    // 3. Falha por Colisão: A chave gerada já existe.
                    attempt++;
                    // Se for a última tentativa, joga um erro mais grave.
                    if (attempt >= MaxRetries)
                    {
                        return StatusCode(500, "Erro crítico: Falha ao gerar chave de API única após múltiplas tentativas.");
                    }
                    // Tenta novamente (o loop continua)
                }
                catch (Exception ex)
                {
                    // Captura qualquer outro erro que não seja de duplicidade
                    return StatusCode(500, $"Erro inesperado durante a geração da chave: {ex.Message}");
                }
            }

            // Este ponto nunca deve ser alcançado, mas é um fallback.
            return StatusCode(500, "Erro interno de lógica de chaves.");
        }

        // Função para gerar uma chave segura
        private string GenerateSecureApiKey(int length = 32)    
        {
            using (var rng = RandomNumberGenerator.Create())//usa o gerador de números aleatórios criptograficamente seguro
            {
                var bytes = new byte[length];//array de bytes
                rng.GetBytes(bytes);//preenche o array com bytes aleatórios
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");//converte para base64 e substitui caracteres para URL-safe
            }
        }
    }
}