using MongoDB.Driver;

namespace API_ovni.Data
{
    public class MongodbService
    {
        public IMongoDatabase Database { get; }

        public MongodbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
            Database = client.GetDatabase(configuration["DatabaseName"]);
        }
    }
}   
