using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization; 

// OvniData: Representa o modelo de dados de um registro de observação (astreamento) da aeronave no sistema.
// Cada propriedade da classe corresponde a um campo do documento armazenado no MongoDB, com mapeamento explícito via atributos [BsonElement].
// Inclui informações como identificador, código hexadecimal, squawk, voo, latitude, longitude, altitude, velocidade, data/hora e outros dados típicos de rastreamento ADS-B.

public class OvniData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")] 
    public string? Id { get; set; }

    [BsonElement("hex_id")]
    [JsonPropertyName("hex_id")] // Traduz o JSON "hex_id" e assim para o restante dos atributos
    public string HexId { get; set; }

    [BsonElement("flight")]
    [JsonPropertyName("flight")] 
    public string Flight { get; set; }

    [BsonElement("alt_baro")]
    [JsonPropertyName("alt_baro")] 
    public double? Alt_baro { get; set; }

    [BsonElement("alt_geom")]
    [JsonPropertyName("alt_geom")] 
    public double? AltGeom { get; set; }

    [BsonElement("ground_speed")]
    [JsonPropertyName("ground_speed")] 
    public double? Ground_Speed { get; set; }

    [BsonElement("indicated_air_speed")]
    [JsonPropertyName("indicated_air_speed")] 
    public double? Indicated_Air_Speed { get; set; }

    [BsonElement("true_air_speed")]
    [JsonPropertyName("true_air_speed")] 
    public double? True_Air_Speed { get; set; }

    [BsonElement("squawk")]
    [JsonPropertyName("squawk")] 
    public int? Squawk { get; set; }

    [BsonElement("track")]
    [JsonPropertyName("track")] 
    public double? Track { get; set; }

    [BsonElement("lat")]
    [JsonPropertyName("lat")] 
    public double? Lat { get; set; }

    [BsonElement("lon")]
    [JsonPropertyName("lon")] 
    public double? Lon { get; set; }

    [BsonElement("emergency")]
    [JsonPropertyName("emergency")] 
    public string? emergency { get; set; }

    [BsonElement("datetime")]
    [JsonPropertyName("datetime")] 
    public DateTime Data { get; set; }
}
//Adicionar Parametros pendentes do ADS-B