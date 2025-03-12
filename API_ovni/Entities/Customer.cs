using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace API_ovni.Entities
{
    public class Customer
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public object CustomerName { get; internal set; }
    }
}
