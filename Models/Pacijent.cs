using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StomatoloskaOrdinacija.Models
{
    [BsonIgnoreExtraElements]

    public class Pacijent
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("Slika")]
        public string Slika { get; set; }

        [BsonElement("Ime")]
        public string Ime { get; set; }
        [BsonElement("Prezime")]
        public string Prezime { get; set; }
        [BsonElement("Adresa")]
        public string Adresa { get; set; }
        [BsonElement("BrojTelefona")]
        public string BrojTelefona { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        [BsonElement("Role")]
        public UserRole Role { get; set; }

        [BsonElement("IstorijaPregleda")]
        public List<ObjectId> IstorijaPregleda { get; set; } = new List<ObjectId>();
    }

    public enum UserRole
    {
        Pacijent,
        Stomatolog
    }
}