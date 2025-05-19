using Microsoft.AspNetCore.Mvc;
using API_ovni.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using System.Net.Http;

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


        /*Construtor: 
         * Recebe o serviço de acesso ao MongoDB e inicializa a coleção para operações.*/
        public OvniDataController(MongodbService mongodbService)
        {
            _ovniDataCollection = mongodbService.Database?.GetCollection<OvniData>("testepy"); //caso precise mudar a coleção ("Nome_Sua_coleção")

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
        [HttpGet("DataPorPeriodo")]
        public async Task<ActionResult<IEnumerable<OvniData>>> GetByPeriod(
            [FromQuery] string dataInicio, // Ex: "2025-05-10"
            [FromQuery] string dataFim     // Ex: "2025-05-14"
)
        {
            // Monta as string completas para o início e fim do período,concertar depois em, usa o datatime .-.
            string dataInicioStr = $"{dataInicio} 00:00:00";
            string dataFimStr = $"{dataFim} 23:59:59";

            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Data, dataInicioStr),
                Builders<OvniData>.Filter.Lte(x => x.Data, dataFimStr)
            );
            var result = await _ovniDataCollection.Find(filter).ToListAsync();
            return Ok(result);
        }



        //pesquisar por range de altitude 
        /*[HttpGet("range")]
        public async Task<IEnumerable<OvniData>> GetByAltitudeRange([FromQuery] double alturaMin, [FromQuery] double alturaMax)
        {
            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Altitude, alturaMin),
                Builders<OvniData>.Filter.Lte(x => x.Altitude, alturaMax)
            );

            return await _ovniDataCollection.Find(filter).ToListAsync();
        }*/

        /*[HttpGet("raw")]
        public async Task<IActionResult> GetRaw()
        {
            try
            {
                var documents = await _ovniDataCollection.Database
                    .GetCollection<BsonDocument>("testepy")
                    .Find(FilterDefinition<BsonDocument>.Empty)
                    .ToListAsync();

                if (!documents.Any())
                {
                    return Ok(new { Message = "Nenhum documento encontrado na coleção." });
                }

                var jsonDocuments = documents.Select(doc => BsonTypeMapper.MapToDotNetValue(doc));
                return Ok(jsonDocuments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar a coleção: {ex.Message}");
            }
        }*/

    }
}
