using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;



// OvniData: Representa o modelo de dados de um registro de observação (astreamento) da aeronave no sistema.
// Cada propriedade da classe corresponde a um campo do documento armazenado no MongoDB, com mapeamento explícito via atributos [BsonElement].
// Inclui informações como identificador, código hexadecimal, squawk, voo, latitude, longitude, altitude, velocidade, data/hora e outros dados típicos de rastreamento ADS-B.

public class OvniData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // Permitir que o Id seja nulo

    [BsonElement("hex_id")]
    public string HexId { get; set; }

   [BsonElement("squawk")]
    public int? Squawk { get; set; }

    [BsonElement("flight")]
    public string Flight { get; set; }

    [BsonElement("lat")]
    public double? Lat { get; set; }

    [BsonElement("lon")]
    public double? Lon { get; set; }

    [BsonElement("altitude")]
    public double? Altitude { get; set; }

    [BsonElement("vert_rate")]
    public int? VertRate { get; set; }

    [BsonElement("track")]
    public double? Track { get; set; }

    [BsonElement("speed")]
    public int? Speed { get; set; }

    [BsonElement("datetime")]
    public string Data { get; set; }




}
//Adicionar Parametros pendentes do ADS-B
