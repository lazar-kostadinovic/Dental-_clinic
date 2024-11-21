using MongoDB.Driver;
using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public class PregledService : IPregledService
    {
        private readonly IMongoCollection<Pregled> _pregledi;

        public PregledService(IOrdinacijaDatabaseSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _pregledi = database.GetCollection<Pregled>(settings.PregledCollectionName);
        }


        public Pregled Create(Pregled pregled)
        {
            _pregledi.InsertOne(pregled);
            return pregled;
        }

        public List<Pregled> Get()
        {
            return _pregledi.Find(pregled => true).ToList();
        }

        public Pregled Get(ObjectId id)
        {
            return _pregledi.Find(pregled => pregled.Id == id).FirstOrDefault();
        }

        public List<Pregled> GetByStomatologId(ObjectId stomatologId)
        {
            var filter = Builders<Pregled>.Filter.Eq(p => p.IdStomatologa, stomatologId);
            return _pregledi.Find(filter).ToList();
        }

        public void Remove(ObjectId id)
        {
            _pregledi.DeleteOne(pregled => pregled.Id == id);
        }

        public void Update(ObjectId id, Pregled pregled)
        {
            _pregledi.ReplaceOne(pregled => pregled.Id == id, pregled);
        }

        public void UpdatePregledStatuses()
        {
            var filter = Builders<Pregled>.Filter.And(
                Builders<Pregled>.Filter.Lt(p => p.Datum, DateTime.UtcNow),
                Builders<Pregled>.Filter.Eq(p => p.Status, StatusPregleda.Predstojeci)
            );

            var update = Builders<Pregled>.Update.Set(p => p.Status, StatusPregleda.Prosli);

            _pregledi.UpdateMany(filter, update);
        }


        public List<Pregled> GetStomatologAppointmentsInDateRange(ObjectId idStomatologa, DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Pregled>.Filter.And(
               Builders<Pregled>.Filter.Eq(p => p.IdStomatologa, idStomatologa),
               Builders<Pregled>.Filter.Gte(p => p.Datum, startDate),
               Builders<Pregled>.Filter.Lte(p => p.Datum, endDate)
           );

            return _pregledi.Find(filter).ToList();
        }
    }
}