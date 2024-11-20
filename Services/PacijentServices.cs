using MongoDB.Driver;
using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public class PacijentService : IPacijentService
    {
        private readonly IMongoCollection<Pacijent> _pacijenti;
        private readonly string _pepper;
        private readonly int _iteration;

        public PacijentService(IOrdinacijaDatabaseSettings settings, IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _pacijenti = database.GetCollection<Pacijent>(settings.PacijentCollectionName);
            _pepper = config["PasswordHasher:Pepper"];
            _iteration = config.GetValue<int>("PasswordHasher:Iteration");
        }

        public Pacijent Create(Pacijent pacijent)
        {
            _pacijenti.InsertOne(pacijent);
            return pacijent;
        }

        public List<Pacijent> Get()
        {
            return _pacijenti.Find(pacijent => true).ToList();
        }

        public Pacijent Get(ObjectId id)
        {
            return _pacijenti.Find(pacijent => pacijent.Id == id).FirstOrDefault();
        }

        public async Task<Pacijent> GetPacijentByEmailAsync(string email)
        {
            return await _pacijenti.Find(pacijent => pacijent.Email == email).FirstOrDefaultAsync();
        }
        public Pacijent GetAndUpdate(ObjectId pacijentId, ObjectId pregledId)
        {
            return _pacijenti.FindOneAndUpdate(
            Builders<Pacijent>.Filter.Eq(p => p.Id, pacijentId),
            Builders<Pacijent>.Update.AddToSet(p => p.IstorijaPregleda, pregledId));
        }

        public void Remove(ObjectId id)
        {
            _pacijenti.DeleteOne(pacijent => pacijent.Id == id);
        }

        public void Update(ObjectId id, Pacijent pacijent)
        {
            _pacijenti.ReplaceOne(pacijent => pacijent.Id == id, pacijent);
        }
        public async Task<bool> Register(RegistrationModel resource)
        {
            var postojeciStomatolog = await _pacijenti.Find(x => x.Email == resource.Email).FirstOrDefaultAsync();
            if (postojeciStomatolog != null)
            {
                throw new InvalidOperationException("Pacijent sa ovom email adresom već postoji.");
            }
            var pacijent = new Pacijent
            {
                Ime = resource.Ime,
                Prezime = resource.Prezime,
                Adresa = resource.Adresa,
                BrojTelefona = resource.BrojTelefona,
                Role = UserRole.Pacijent,
                Email = resource.Email,
                PasswordSalt = PasswordHasher.GenerateSalt(),
            };
            pacijent.Password = PasswordHasher.ComputeHash(resource.Password, pacijent.PasswordSalt, _pepper, _iteration);
            _pacijenti.InsertOne(pacijent);

            return true;
        }
        public bool RemoveAppointment(ObjectId pacijentId, ObjectId appointmentId)
        {
            var filter = Builders<Pacijent>.Filter.Eq(p => p.Id, pacijentId);
            var update = Builders<Pacijent>.Update.Pull(p => p.IstorijaPregleda, appointmentId);
            var result = _pacijenti.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }

        public bool AddOrUpdateSlika(ObjectId pacijentId, string slikaFileName)
        {
            var filter = Builders<Pacijent>.Filter.Eq(p => p.Id, pacijentId);
            var update = Builders<Pacijent>.Update.Set(p => p.Slika, slikaFileName);
            var result = _pacijenti.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }

    }
}