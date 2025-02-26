using System;
using MongoDB.Bson;
using MongoDB.Driver;

public class Consulta
{
    //Altura Max e Min de voos
    public double AlturaMax;
    public double AlturaMin;
    public String Hexid;

    public Consulta(double AlturaMax, double AlturaMin, String Hexid)
    {

        this.AlturaMax = AlturaMax;
        this.AlturaMin = AlturaMin;
        this.Hexid = Hexid;
    }
    //metodos get e set
    public double getAlturaMax()
    {
        return AlturaMax;
    }
    public void setAlturaMax(double AlturaMax)
    {
        this.AlturaMax = AlturaMax;
    }
    public double getAlturaMin()
    {
        return AlturaMin;
    }
    public void setAlturaMin(double AlturaMin)
    {
        this.AlturaMin = AlturaMin;
    }
    public String getHexid()
    {
        return Hexid;
    }
    public void setHexid(String Hexid)
    {
        this.Hexid = Hexid;
    }


    //metodo para conectar com o banco de dados MOngoDB Atlas
    public MongoClient ConectarMongoDB()
    {
        const string connectionUri = "mongodb+srv://kathleenkat:pamonha@cluster0.vjanz.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);

        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao tentar conectar ao MongoDB: {ex.Message}");
        }

        return client;

    }

    //metodo para consultar os dados no banco de dados MongoDB Atlas
    public void ConsultarRetornar()
    {
        var client = ConectarMongoDB(); // Obter o cliente conectado

        // Acessar o banco de dados e a coleção
        var database = client.GetDatabase("minhaBase");
        var collection = database.GetCollection<BsonDocument>("testepy");

        // Definir o filtro da consulta (modifique conforme necessário)
        var filter = Builders<BsonDocument>.Filter.Eq("hex_id", this.Hexid);

        // Executar a consulta
        var documentos = collection.Find(filter).ToList();

        // Processar os documentos retornados
        foreach (var doc in documentos)
        {
           doc.ToString();

        }



        
    }

}
