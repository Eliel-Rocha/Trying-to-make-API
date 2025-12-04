using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// Este modelo representa o documento que será salvo na coleção "apiKeys"
// no MongoDB, armazenando a chave gerada.
namespace API_ovni.Models;
public class ApiKeyUser
{
    [BsonElement("isAdmin")]
    public bool IsAdmin { get; set; } = false;

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("apiKeyHash")]
    public string ApiKeyHash { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } // Nome completo do usuário

    [BsonElement("email")]
    public string Email { get; set; } // Email do usuário

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    

}