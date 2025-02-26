using System;

using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MongoDBService
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoDBService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDB"));
        var database = client.GetDatabase("minhaBase");
        _collection = database.GetCollection<BsonDocument>("testepy");
    }

    public async Task<List<BsonDocument>> ConsultarRetornarAsync(string hexid)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("hex_id", hexid);
        return await _collection.Find(filter).ToListAsync();
    }
}
