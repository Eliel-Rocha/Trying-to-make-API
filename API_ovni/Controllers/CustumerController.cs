using Microsoft.AspNetCore.Mvc;
using API_ovni.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;

namespace API_ovni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvniDataController : ControllerBase
    {
        private readonly IMongoCollection<OvniData> _ovniDataCollection;

        public OvniDataController(MongodbService mongodbService)
        {
            _ovniDataCollection = mongodbService.Database?.GetCollection<OvniData>("testepy");//colection do banco
        }

        [HttpGet]
        public async Task<IEnumerable<OvniData>> Get()
        {
            return await _ovniDataCollection.Find(FilterDefinition<OvniData>.Empty).ToListAsync();
        }





        //pesquisar por id

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<OvniData?>> GetById(string id)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Id, id);
            var ovniData = await _ovniDataCollection.Find(filter).FirstOrDefaultAsync();
            return ovniData is not null ? Ok(ovniData) : NotFound();
        }

        //pesquisa de range de Periodo ---> dia/mes/ano)
        [HttpGet("Data")]
        public async Task<ActionResult<IEnumerable<OvniData>>> GetByDate([FromQuery] string dataInicio, [FromQuery] string dataFim)
        {
            if (!DateTime.TryParseExact(dataInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataInicioParsed) ||
                !DateTime.TryParseExact(dataFim, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataFimParsed))
            {
                return BadRequest("Datas devem estar no formato dd/MM/yyyy");
            }

            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Data, dataInicioParsed),
                Builders<OvniData>.Filter.Lte(x => x.Data, dataFimParsed)
            );
            var result = await _ovniDataCollection.Find(filter).ToListAsync();
            return Ok(result);
        }



        //pesquisa de Companhia aerea --->  pelo HexID ou Numero de serie
        [HttpGet("Companhia Aerea")]
        public async Task<IEnumerable<OvniData>> GetByCompanhia([FromQuery] string companhia)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.CompanhiaAerea, companhia);
            return await _ovniDataCollection.Find(filter).ToListAsync();
        }
        //pesquisa de Aeroportos -------->  Origem e destino
        [HttpGet("Origem")]
        public async Task<IEnumerable<OvniData>> GetByOrigem([FromQuery] string origem)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Origem, origem);
            return await _ovniDataCollection.Find(filter).ToListAsync();
        }
        [HttpGet("Destino")]

        public async Task<IEnumerable<OvniData>> GetByDestino([FromQuery] string destino)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Destino, destino);
            return await _ovniDataCollection.Find(filter).ToListAsync();
        }


        //pesquisar por range de altitude 
        [HttpGet("range")]
        public async Task<IEnumerable<OvniData>> GetByAltitudeRange([FromQuery] double alturaMin, [FromQuery] double alturaMax)
        {
            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Altitude, alturaMin),
                Builders<OvniData>.Filter.Lte(x => x.Altitude, alturaMax)
            );

            return await _ovniDataCollection.Find(filter).ToListAsync();
        }



        //criar um novo objeto ----> adicionar no bancoMongoDbAtlas
        [HttpPost]
        public async Task<ActionResult> Create(OvniData ovniData)
        {
         
            
            ovniData.Id = ObjectId.GenerateNewId().ToString();
           

            ovniData.Data = DateTime.Today.AddDays(-1); // Define a data atual

            await _ovniDataCollection.InsertOneAsync(ovniData);
            return CreatedAtAction(nameof(GetById), new { id = ovniData.Id.ToString() }, ovniData);
        }

        //atualizar um objeto
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult> Update(string id, OvniData ovniData)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Id, id);
            var existingOvniData = await _ovniDataCollection.Find(filter).FirstOrDefaultAsync();

            if (existingOvniData is null)
            {
                return NotFound();
            }

            await _ovniDataCollection.ReplaceOneAsync(filter, ovniData);
            return Ok();
        }



        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult> Delete(string id)//deletar um objeto
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Id, id);
            var result = await _ovniDataCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return Ok();
        }


    }
}
