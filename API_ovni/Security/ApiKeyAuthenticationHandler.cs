using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text.Encodings.Web;
using API_ovni.Models;

namespace API_ovni.Security
{
    
    //intercepta a requisição e verifica o Header "X-Api-Key"
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        //essa parte define o nome do header que será usado para a autenticação
        private const string API_KEY_HEADER = "X-Api-Key";
        private readonly IConfiguration _config;
        private readonly IMongoCollection<ApiKeyUser> _userCollection;


        
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config,
            IMongoCollection<ApiKeyUser> userCollection)
            : base(options, logger, encoder, clock)
        {
            _config = config;
            _userCollection = userCollection;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //EXTRAÇÃO DO HEADER: para verificar se a chave de API está presente na requisição
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyFromHeader))
            {
                return AuthenticateResult.Fail("Header X-Api-Key ausente.");
            }

            var apiKey = apiKeyFromHeader.ToString();
            string role;

            //****Checa se é o ADMIN (Chave Mestra do appsettings.json)******
            var adminKey = _config.GetValue<string>("Authentication:AdminApiKey");
            if (apiKey == adminKey)
            {
                
                role = "Admin";
                return CreateSuccessResult(role);
            }

            // USUÁRIO PÚBLICO (do banco de dados)
            var userKey = await _userCollection.Find(u => u.ApiKey == apiKey).FirstOrDefaultAsync();
            if (userKey != null)
            {
                // Verifica o privilégio no banco de dados
                role = userKey.IsAdmin ? "Admin" : "User";
                return CreateSuccessResult(role);
            }

            return AuthenticateResult.Fail("Chave de API inválida.");
        }


        // Cria o resultado de autenticação com a role apropriada
        private AuthenticateResult CreateSuccessResult(string role)
        {
            var claims = new[] { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
    }
}