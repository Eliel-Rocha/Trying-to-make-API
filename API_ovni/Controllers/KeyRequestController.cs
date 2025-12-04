using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using API_ovni.Models;
using API_ovni.Services; // Necessário para ApiKeyService

// Esta classe é o controller para operações públicas, como gerar chaves de API para usuários entre outros.
namespace API_ovni.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class KeyRequestController : ControllerBase 
    {
        private readonly IMongoCollection<ApiKeyUser> _userCollection;
        private readonly ApiKeyService _apiKeyService;

        // CONSTRUTOR
        public KeyRequestController(
            IMongoCollection<ApiKeyUser> userCollection,
            ApiKeyService apiKeyService)
        {
            _userCollection = userCollection;
            _apiKeyService = apiKeyService; // Atribuição correta
        }

        /// <summary>
        /// (PÚBLICO) Gera uma nova chave de API (somente leitura)
        /// </summary>
        [HttpPost("GenerateKey")]
        public async Task<IActionResult> GenerateKey([FromBody] ApiKeyRequest request)
        {
            const int MaxRetries = 3;
            int attempt = 0;//contador de tentativas
            string newKey;

            var fullName = $"{request.FirstName} {request.LastName}";
            var email = request.Email;

            while (attempt < MaxRetries)
            {
                try
                {
                    
                    newKey = _apiKeyService.GenerateSecureApiKey();
                    var keyHash = _apiKeyService.HashApiKey(newKey);
                    var newUser = new ApiKeyUser

                    {
                        ApiKeyHash = keyHash, 
                        Name = fullName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    };

                    //Tenta inserir no banco
                    await _userCollection.InsertOneAsync(newUser);

                    //Retorna a chave e sai do loop
                    return Ok(new { apiKey = newKey });
                }
                catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Code == 11000)
                {

                    if (ex.Message.Contains("email"))
                    {
                        // Se foi o e-mail, não adianta tentar de novo. Retorna erro 400 (Bad Request).
                        return BadRequest(new { message = "Este e-mail já possui uma chave de API cadastrada." });
                    }


                    //A chave gerada já existe.
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
                    // Captura qualquer outro erro
                    return StatusCode(500, $"Erro inesperado durante a geração da chave: {ex.Message}");
                }
            }

            return StatusCode(500, "Erro interno de lógica de chaves.");
        }
    }
}