using Microsoft.AspNetCore.Mvc;
using API_ovni.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using System.Net.Http;
using API_ovni.Services;

namespace API_ovni.Controllers
{

    // API_ovni.Controllers: Responsável por controlar os endpoints da API,
    // processar as requisições HTTP e interagir com o banco de dados MongoDB
    // para fornecer, buscar ou manipular dados conforme as operações solicitadas.



    [Route("api/[controller]")]
    [ApiController]
    public class OvniDataController : ControllerBase
    {
        private readonly IMongoCollection<OvniData> _ovniDataCollection;

        private readonly LimpezaArmazenamentoService _limpezaArmazenamentoService;        /*Construtor: 
         * Recebe o serviço de acesso ao MongoDB e inicializa a coleção para operações.*/
        public OvniDataController(
            IMongoCollection<OvniData> ovniDataCollection,
            LimpezaArmazenamentoService limpezaArmazenamentoService)
        {
            _ovniDataCollection = ovniDataCollection;
            _limpezaArmazenamentoService = limpezaArmazenamentoService;
        }


        //pesquisar por id: filtro para o campo ID criado automaticamente pelo mongodb
        /*retorna o documento enontrado ou se não NotFound()*/

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<OvniData?>> GetById(string id)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Id, id);
            var ovniData = await _ovniDataCollection.Find(filter).FirstOrDefaultAsync();
            return ovniData is not null ? Ok(ovniData) : NotFound();
        }


        //pesquisa de range de Periodo ---> dia/mes/ano)
        /// <summary>
        /// Busca de dados dentro de um período específico.
        /// </summary>
        /// <param name="dataInicio">A data de início da busca (formato: yyyy-MM-dd).</param>
        /// <param name="dataFim">A data de fim da busca (formato: yyyy-MM-dd).</param>
        /// <returns>Uma lista de avistamentos encontrados no período.</returns>
        /// <response code="200">Retorna a lista de avistamentos.</response>
        /// <response code="400">Se o formato da data for inválido.</response>
        [HttpGet("DataPorPeriodo")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Informa o tipo de retorno para o status 200
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OvniData>>> GetByPeriod(


            [FromQuery] string dataInicio, // Ex: "2025-05-10"
            [FromQuery] string dataFim     // Ex: "2025-05-14"
)
        {
            if (!DateTime.TryParseExact(dataInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtInicio))
            {
                return BadRequest("Formato de dataInicio inválido. Use yyyy-MM-dd.");
            }

            if (!DateTime.TryParseExact(dataFim, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtFim))
            {
                return BadRequest("Formato de dataFim inválido. Use yyyy-MM-dd.");
            }

            DateTime dtFimFinal = dtFim.AddDays(1);

            // Agora o filtro compara DateTime (x.Data) com DateTime (dtInicio/dtFimFinal)
            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Data, dtInicio),
                Builders<OvniData>.Filter.Lt(x => x.Data, dtFimFinal)
            );

            var result = await _ovniDataCollection.Find(filter).ToListAsync();
            return Ok(result);
        }



        //API para inserção de documentos em lote
        //recebe uma lista de objetos OvniData no corpo da requisição e insere todos na coleção MongoDB.

        [HttpPost("InserirDocumento")]
        public async Task<ActionResult> InsertBatch([FromBody] List<OvniData> newOvniDataList)
        {
            if (newOvniDataList == null || !newOvniDataList.Any())
                return BadRequest("A lista de documentos não pode ser nula ou vazia.");

            try
            {
                Console.WriteLine("Verificando necessidade de limpeza...");
                await _limpezaArmazenamentoService.Limpar(newOvniDataList.Count);
                // Aguarda a limpeza antes de inserir novos documentos

                Console.WriteLine("Inserindo documentos...");
                await _ovniDataCollection.InsertManyAsync(newOvniDataList);
                Console.WriteLine("Inserção concluída com sucesso.");

                return Ok(new { message = $"{newOvniDataList.Count} documentos inseridos com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inserir documentos: {ex}");
                return StatusCode(500, $"Erro interno ao processar a requisição: {ex.Message}");
            }
        }

    }
}
