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

            // se e admin
            var adminKey = _config.GetValue<string>("Authentication:AdminApiKey");
            if (apiKeyIncoming == adminKey)
            {
                return CreateSuccessResult("Admin");
            }

            
            // Gera o Hash da chave recebida para comparar com o banco
            var incomingHash = _apiKeyService.HashApiKey(apiKeyIncoming);
            var userKey = await _userCollection.Find(u => u.ApiKeyHash == incomingHash).FirstOrDefaultAsync();

            if (userKey != null)
            {
                string role = userKey.IsAdmin ? "Admin" : "User";
                return CreateSuccessResult(role);
            }

            return AuthenticateResult.Fail("Chave de API inválida.");
        }

        private AuthenticateResult CreateSuccessResult(string role)
        {
            var claims = new[] { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
    }
}