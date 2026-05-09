using Microsoft.AspNetCore.Mvc;
using API_ovni.Models;
using API_ovni.Services;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace API_ovni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyRequestController : ControllerBase
    {
        private readonly ApiKeyService _apiKeyService;
        private readonly IMongoCollection<ApiKeyUser> _userCollection;
        private readonly IConfiguration _config;

        public KeyRequestController(ApiKeyService apiKeyService, IMongoCollection<ApiKeyUser> userCollection, IConfiguration config)
        {
            _apiKeyService = apiKeyService;
            _userCollection = userCollection;
            _config = config;
        }

        // GERA A CHAVE (Cadastro)
        [HttpPost("GenerateKey")]
        public async Task<IActionResult> GenerateKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                // Gera uma chave segura e armazena o hash no banco
                var newApiKey = _apiKeyService.GenerateSecureApiKey();
                var newUser = new ApiKeyUser
                {
                    Name = request.FirstName + " " + request.LastName,
                    Email = request.Email,
                    ApiKeyHash = _apiKeyService.HashApiKey(newApiKey),
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow,
                    Validade = DateTime.UtcNow.AddMonths(6),
                    IsActive = true
                };

                await _userCollection.InsertOneAsync(newUser);

                return Ok(new { apiKey = newApiKey, name = newUser.Name, email = newUser.Email });
            }
            catch (MongoWriteException ex) when (ex.WriteError.Code == 11000)
            {
                return BadRequest(new { message = "Este e-mail já possui uma chave cadastrada. Use a chave existente ou peça suporte." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao gerar chave.", detail = ex.Message });
            }
        }

        // VALIDA A CHAVE (Login)
        [HttpGet("Validate")]
        public async Task<IActionResult> ValidateKey([FromQuery] string apiKey)
        {
            // Checa Admin Mestre
            var adminKey = _config.GetValue<string>("Authentication:AdminApiKey");
            if (apiKey == adminKey) return Ok(new { role = "admin", name = "Administrador" });

            // Checa Banco
            var hash = _apiKeyService.HashApiKey(apiKey);

            try { 
                var user = await _userCollection.Find(u => u.ApiKeyHash == hash).FirstOrDefaultAsync();

                if (user == null) return Unauthorized(new { message = "Chave não encontrada." });

                // --- LÓGICA DE REACTIVAÇÃO ---
                // Se a chave exist, mas venceu ou está inativa
                if (!user.IsActive || user.Validade < DateTime.UtcNow)
                {
                    // Retorna um código especial (403) e um aviso que pode reativar
                    return StatusCode(403, new
                    {
                        message = "Chave expirada.",
                        canReactivate = true, 
                        validade = user.Validade
                    });
                }

                return Ok(new { role = user.IsAdmin ? "admin" : "user", name = user.Name });
            }
            catch (OperationCanceledException) // Ou TimeoutException
            {
                return StatusCode(503, new { message = "O servidor de banco de dados não respondeu a tempo." });
            }
        }

        //  REATIVAR CHAVE (Renovação)
        [HttpPost("Reactivate")]
        public async Task<IActionResult> ReactivateKey([FromBody] string apiKey)
        {
            var hash = _apiKeyService.HashApiKey(apiKey);
            var user = await _userCollection.Find(u => u.ApiKeyHash == hash).FirstOrDefaultAsync();

            if (user == null) return NotFound("Chave não encontrada.");

          

            // Ativa e dá +6 meses a partir da data atual
            var update = Builders<ApiKeyUser>.Update
                .Set(u => u.IsActive, true)
                .Set(u => u.Validade, DateTime.UtcNow.AddMonths(6));

            await _userCollection.UpdateOneAsync(u => u.ApiKeyHash == hash, update);

            return Ok(new { message = "Chave reativada com sucesso! Validade estendida por 6 meses." });
        }
    }

    public class ApiKeyRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}