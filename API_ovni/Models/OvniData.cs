using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class OvniData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // Permitir que o Id seja nulo

    [BsonElement("hex_id")]
    public string HexId { get; set; }

    [BsonElement("squawk")]
    public int Squawk { get; set; }

    [BsonElement("flight")]
    public string Flight { get; set; }

    [BsonElement("lat")]
    public double Lat { get; set; }

    [BsonElement("lon")]
    public double Lon { get; set; }

    [BsonElement("altitude")]
    public double Altitude { get; set; }

    [BsonElement("vert_rate")]
    public int VertRate { get; set; }

    [BsonElement("track")]
    public double Track { get; set; }

    [BsonElement("speed")]
    public int Speed { get; set; }

    [BsonElement("Origem")]
    public String Origem { get; set; }

    [BsonElement("Destino")]
    public String Destino { get; set; }

    [BsonElement("data")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local, DateOnly = true)]
    public DateTime Data { get; set; }

    [BsonElement("Companhia Aerea")]
    public String CompanhiaAerea { get; set; }
}
//Adicionar Parametros pendentes do ADS-B
