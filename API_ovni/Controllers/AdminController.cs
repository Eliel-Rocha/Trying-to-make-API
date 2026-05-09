using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using API_ovni.Models;
using API_ovni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

// Esta classe é o controller para operações administrativas, como gerar chaves de API para usuários.

namespace API_ovni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")] // SÓ ADMIN PODE ACESSAR ESTE CONTROLLER
    public class AdminController : ControllerBase
    {
        private readonly IMongoCollection<ApiKeyUser> _userCollection;
        private readonly IMongoCollection<ConfiguracaoLimpeza> _configCollection;// Coleção de configuração
        private readonly LimpezaArmazenamentoService _limpezaService;
        private readonly ApiKeyService _apiKeyService;

        public AdminController(
            IMongoCollection<ApiKeyUser> userCollection,
            IMongoCollection<ConfiguracaoLimpeza> configCollection,
            LimpezaArmazenamentoService limpezaService,
            ApiKeyService apiKeyService)
        {
            _userCollection = userCollection;
            _configCollection = configCollection;
            _limpezaService = limpezaService;
            _apiKeyService = apiKeyService;
        }

        // GERAÇÃO DE CHAVE E CONCESSÃO DE ADMIN 

        /// <summary>
        /// (ADMIN) Gera uma nova chave de API para um usuário e, opcionalmente, concede privilégios de Admin.
        /// </summary>
        /// <param name="ownerInfo">O nome do usuário/pesquisador.</param>
        /// <param name="grantAdmin">Define se a chave deve ter privilégios de Admin (true) ou User (false) na criação.</param>
        [HttpPost("GerarChaveUsuario")]
        public async Task<IActionResult> GenerateUserKey([FromBody] string ownerInfo, [FromQuery] bool grantAdmin = false)
        {
            const int MaxRetries = 3;
            int attempt = 0;

            // Separa as informações (Nome e Email placeholder)
            var parts = ownerInfo.Split(new char[] { ',' }, 2);
            var name = parts[0].Trim();
            var email = parts.Length > 1 ? parts[1].Trim() : $"{name.Replace(" ", "").ToLower()}@admin.temp";

            while (attempt < MaxRetries)
            {
                try
                {
                    var newKey = _apiKeyService.GenerateSecureApiKey();

                    // Hash
                    var keyHash = _apiKeyService.HashApiKey(newKey);

                    var newUser = new ApiKeyUser
                    {
                        //Salva o HASH, não a chave
                        ApiKeyHash = keyHash,
                        Name = name,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        IsAdmin = grantAdmin,

                        Validade = DateTime.UtcNow.AddYears(1), // Admin dá 1 ano de validade
                        IsActive = true
                    };

                    await _userCollection.InsertOneAsync(newUser);

                    string role = grantAdmin ? "Admin" : "User";

                    return Ok(new
                    {
                        message = $"Chave de {role} criada com sucesso.",
                        name = name,
                        email = email,
                        apiKey = newKey
                    });
                }
                catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Code == 11000)
                {
                    attempt++;
                    if (attempt >= MaxRetries)
                    {
                        return StatusCode(500, "Erro crítico: Falha ao gerar chave de API única após múltiplas tentativas.");
                    }
                    if (ex.WriteError.Message.Contains("email"))
                    {
                        return BadRequest(new { message = "Este e-mail já está cadastrado em outra chave." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erro inesperado durante a geração da chave: {ex.Message}");
                }
            }
            return StatusCode(500, "Erro interno de lógica de chaves.");
        }


        //GERENCIAMENTO DE CONFIGURAÇÃO DE LIMPEZA

        /// <summary>
        /// (ADMIN) Obtém a configuração atual de limpeza para exibir na UI.
        /// </summary>
        

        [HttpGet("ConfiguracaoLimpeza")]
        public async Task<IActionResult> GetLimpezaConfig()
        {
            // Busca Configuração
            var config = await _configCollection.Find(x => x.Id == "config_adsb").FirstOrDefaultAsync();

            if (config == null)
            {
                // Se não existir
                config = new ConfiguracaoLimpeza { LimiteMaximoBytes = 104857600, PercentualAlvoOcupacao = 0.7 };
            }

            // Busca Tamanho Atual 
            long bytesAtuais = await _limpezaService.ObterTamanhoAtualAsync();

            // Calcula Porcentagem Usada
            double usoPorcentagem = 0;
            if (config.LimiteMaximoBytes > 0)
            {
                usoPorcentagem = (double)bytesAtuais / config.LimiteMaximoBytes * 100;
            }

            // Retorna tudo num objeto combinado
            return Ok(new
            {
                config = config,
                status = new
                {
                    bytesAtuais = bytesAtuais,
                    usoPorcentagem = Math.Round(usoPorcentagem, 2) // Arredonda para 2 casas decimais
                }
            });
        }

        /// <summary>
        /// (ADMIN) Atualiza os parâmetros de exclusão do banco de dados.
        /// </summary>
        [HttpPost("ConfiguracaoLimpeza")]
        public async Task<IActionResult> UpdateLimpezaConfig([FromBody] ConfigUpdateDto dto)
        {
            // O DTO já garante que os campos são válidos (Required e Range)

            var update = Builders<ConfiguracaoLimpeza>.Update
                .Set(c => c.LimiteMaximoBytes, dto.LimiteMaximoBytes)
                .Set(c => c.PercentualAlvoOcupacao, dto.PercentualAlvoOcupacao);

            // Encontra pelo ID fixo e aplica a atualização
            var result = await _configCollection.UpdateOneAsync(c => c.Id == "config_adsb", update);

            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Documento de configuração não encontrado para atualização." });
            }

            return Ok(new { message = "Configuração de limpeza atualizada com sucesso!" });
        }

        /// <summary>
        /// Força a limpeza do banco de dados removendo a quantidade de dados 
        /// necessária para atingir a 'Meta de Ocupação' configurada.
        /// </summary>

        [HttpPost("ExecutarLimpeza")]
        public async Task<IActionResult> ExecuteLimpeza()
        {
            try
            {
                await _limpezaService.Limpar(0);
                return Ok(new { message = "Processo de limpeza iniciado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao iniciar o processo de limpeza: {ex.Message}");
            }
        }
    }


    // Este DTO é usado pelo endpoint UpdateLimpezaConfig para receber os dados do Frontend.
    public class ConfigUpdateDto
    {
        [Required]
        public long LimiteMaximoBytes { get; set; }

        [Required]
        [Range(0.1, 1.0)]
        public double PercentualAlvoOcupacao { get; set; }
    }
}