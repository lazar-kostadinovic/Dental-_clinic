using MongoDB.Driver;
using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public class OcenaStomatologaService : IOcenaStomatologaService
    {
        private readonly IMongoCollection<OcenaStomatologa> _ocene;

        public OcenaStomatologaService(IOrdinacijaDatabaseSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _ocene = database.GetCollection<OcenaStomatologa>(settings.OcenaStomatologaCollectionName);
        }


        public OcenaStomatologa Create(OcenaStomatologa ocena)
        {
            _ocene.InsertOne(ocena);
            return ocena;
        }

        public List<OcenaStomatologa> Get()
        {
            return _ocene.Find(ocena => true).ToList();
        }

        public OcenaStomatologa Get(ObjectId id)
        {
            return _ocene.Find(ocena => ocena.Id == id).FirstOrDefault();
        }

        public void Remove(ObjectId id)
        {
            _ocene.DeleteOne(ocena => ocena.Id == id);
        }

        public void Update(ObjectId id, OcenaStomatologa novaOcena)
        {
            _ocene.ReplaceOne(ocena => ocena.Id == id, novaOcena);
        }

        public List<OcenaStomatologa> GetReviews(ObjectId idStomatologa)
        {
            return _ocene.Find(ocena => ocena.IdStomatologa == idStomatologa).ToList();
        }
    }
}