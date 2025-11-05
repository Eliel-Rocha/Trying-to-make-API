using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class ConfiguracaoLimpeza
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    [BsonElement("descricao")]
    public string Descricao { get; set; }

    [BsonElement("limiteMaximoBytes")]
    public long LimiteMaximoBytes { get; set; }

    [BsonElement("percentualAlvoOcupacao")]
    public double PercentualAlvoOcupacao { get; set; }
}