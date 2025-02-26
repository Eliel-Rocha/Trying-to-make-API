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
    public void ConectarMongoDB()
    {
        const string connectionUri = "mongodb+srv://kathleenkat:pamonha@cluster0.vjanz.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        var client = new MongoClient(connectionUri);
        var database = client.GetDatabase("minhaBase");
        var collection = database.GetCollection<BsonDocument>("testepy");

        // Mensagem de conectado
        Console.WriteLine("Conectado ao MongoDB");

        var filter = Builders<BsonDocument>.Filter.Eq("hex_id", Hexid);
        var dados = collection.Find(filter).ToList();

        // Verificar se encontrou dados
        if (dados != null && dados.Count > 0)
        {
            Console.WriteLine("Dados encontrados:");
            foreach (var doc in dados)
            {
                Console.WriteLine(doc.ToJson());
            }
        }
        else
        {
            Console.WriteLine("Dados não encontrados");
        }

    }
}