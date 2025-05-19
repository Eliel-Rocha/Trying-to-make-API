using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace API_ovni.Entities
// API_ovni.Entities.Customer: Representa a entidade "Customer" (cliente) no sistema.
// Mapeia o documento da coleção correspondente no MongoDB, incluindo o identificador (_id) e o nome do cliente.

{
    public class Customer
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public object? CustomerName { get; internal set; }
    }
}
