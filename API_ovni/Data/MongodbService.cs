using MongoDB.Driver;

// API_ovni.Data.MongodbService: Serviço responsável por gerenciar a conexão com o banco de dados MongoDB.
// Fornece acesso à instância do banco (IMongoDatabase)
// para que outros componentes da aplicação possam realizar operações de leitura e escrita.

namespace API_ovni.Data
{
    public class MongodbService
    {
        public IMongoDatabase Database { get; }

        public MongodbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb"); //esta é a string de conexão, é definida no appsettings.json
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase("minhaBase"); //("minhaBase") nome referente ao banco utilizado no MongodbAltas,caso seja outr apenas mude: ("Nome_do_Banco")
        }
    }
}   
