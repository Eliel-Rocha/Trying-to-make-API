using Microsoft.AspNetCore.Mvc;
using API_ovni.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace API_ovni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvniDataController : ControllerBase
    {
        private readonly IMongoCollection<OvniData> _ovniDataCollection;

        public OvniDataController(MongodbService mongodbService)
        {
            _ovniDataCollection = mongodbService.Database?.GetCollection<OvniData>("testepy");
        }

        [HttpGet]
        public async Task<IEnumerable<OvniData>> Get()
        {
            return await _ovniDataCollection.Find(FilterDefinition<OvniData>.Empty).ToListAsync();
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<OvniData?>> GetById(string id)
        {
            var filter = Builders<OvniData>.Filter.Eq(x => x.Id, id);
            var ovniData = await _ovniDataCollection.Find(filter).FirstOrDefaultAsync();
            return ovniData is not null ? Ok(ovniData) : NotFound();
        }

        [HttpGet("range")]
        public async Task<IEnumerable<OvniData>> GetByAltitudeRange([FromQuery] double alturaMin, [FromQuery] double alturaMax)
        {
            var filter = Builders<OvniData>.Filter.And(
                Builders<OvniData>.Filter.Gte(x => x.Altitude, alturaMin),
                Builders<OvniData>.Filter.Lte(x => x.Altitude, alturaMax)
            );

            return await _ovniDataCollection.Find(filter).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> Create(OvniData ovniData)
        {
            if (string.IsNullOrEmpty(ovniData.Id))
            {
                ovniData.Id = ObjectId.GenerateNewId().ToString();
            }

            await _ovniDataCollection.InsertOneAsync(ovniData);
            return CreatedAtAction(nameof(GetById), new { id = ovniData.Id }, ovniData);
        }

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
        public async Task<ActionResult> Delete(string id)
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
