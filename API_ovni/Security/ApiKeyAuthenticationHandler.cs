using System.Security.Claims;
using System.Text.Encodings.Web;
using API_ovni.Models;
using API_ovni.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API_ovni.Security
{
    // intercepta a requisição e verifica o Header "X-Api-Key"
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string API_KEY_HEADER = "X-Api-Key";
        private readonly IConfiguration _config;
        private readonly IMongoCollection<ApiKeyUser> _userCollection;
        private readonly ApiKeyService _apiKeyService;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config,
            IMongoCollection<ApiKeyUser> userCollection,
            ApiKeyService apiKeyService)
            : base(options, logger, encoder, clock)
        {
            _config = config;
            _userCollection = userCollection;
            _apiKeyService = apiKeyService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyFromHeader))
            {
                return AuthenticateResult.Fail("Header X-Api-Key ausente.");
            }

            var apiKeyIncoming = apiKeyFromHeader.ToString();

            // CHAVE MESTRA (Sempre ativa)
            var adminKey = _config.GetValue<string>("Authentication:AdminApiKey");
            if (!string.IsNullOrEmpty(adminKey) && apiKeyIncoming == adminKey)
            {
                return CreateSuccessResult("Admin", "Administrador Mestre (Root)");
            }

            // BUSCA NO BANCO
            var incomingHash = _apiKeyService.HashApiKey(apiKeyIncoming);
            var userKey = await _userCollection.Find(u => u.ApiKeyHash == incomingHash).FirstOrDefaultAsync();

            if (userKey == null)
            {
                return AuthenticateResult.Fail("Chave de API inválida.");
            }

            
            // LÓGICA DE LIMPEZA 
           
            if (userKey.Validade < DateTime.UtcNow.AddDays(-90))
            {
                // Remove do banco de dados
                await _userCollection.DeleteOneAsync(u => u.ApiKeyHash == userKey.ApiKeyHash);
                return AuthenticateResult.Fail("Esta chave foi excluída permanentemente por inatividade.");
            }
            // LÓGICA DE DESATIVAÇÃO 
            if (userKey.Validade < DateTime.UtcNow)
            {
                if (userKey.IsActive) 
                {
                    // Desativa no banco
                    var update = Builders<ApiKeyUser>.Update.Set(u => u.IsActive, false);
                    await _userCollection.UpdateOneAsync(u => u.ApiKeyHash == userKey.ApiKeyHash, update);
                }

                return AuthenticateResult.Fail($"Chave expirada e desativada em: {userKey.Validade}. Contate o suporte para reativar.");
            }

           
            // LÓGICA DE BLOQUEIO 
       
            if (!userKey.IsActive)
            {
                return AuthenticateResult.Fail("Esta chave está desativada.");
            }

            
            string role = userKey.IsAdmin ? "Admin" : "User";
            string nomeReal = userKey.Name ?? "Usuário Sem Nome";

            return CreateSuccessResult(role, nomeReal);
        }

        
       
        private AuthenticateResult CreateSuccessResult(string role, string name)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, name) 
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
    }
}