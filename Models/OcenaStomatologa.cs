using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StomatoloskaOrdinacija.Models
{
    [BsonIgnoreExtraElements]
    public class OcenaStomatologa
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("IdStomatologa")]
        public ObjectId IdStomatologa { get; set; }
        [BsonElement("IdPacijenta")]
        public ObjectId IdPacijenta { get; set; }
        [BsonElement("Datum")]
        public DateTime Datum { get; set; }
        [BsonElement("Komentar")]
        public string Komentar { get; set; }
        [BsonElement("Ocena")]
        public int Ocena { get; set; }

    }
}



