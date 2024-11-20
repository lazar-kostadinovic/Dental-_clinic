using MongoDB.Driver;
using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public class StomatologService : IStomatologService
    {
        private readonly IMongoCollection<Stomatolog> _stomatolozi;

        private readonly string _pepper;
        private readonly int _iteration;

        public StomatologService(IOrdinacijaDatabaseSettings settings, IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _stomatolozi = database.GetCollection<Stomatolog>(settings.StomatologCollectionName);
            _pepper = config["PasswordHasher:Pepper"];
            _iteration = config.GetValue<int>("PasswordHasher:Iteration");

        }

        public Stomatolog Create(Stomatolog stomatolog)
        {
            _stomatolozi.InsertOne(stomatolog);
            return stomatolog;
        }

        public List<Stomatolog> Get()
        {
            return _stomatolozi.Find(stomatolog => true).ToList();
        }

        public Stomatolog Get(ObjectId id)
        {
            return _stomatolozi.Find(stomatolog => stomatolog.Id == id).FirstOrDefault();
        }
        
        public async Task<Stomatolog> GetStomatologByEmailAsync(string email)
        {
            return await _stomatolozi.Find(stomatolog => stomatolog.Email == email).FirstOrDefaultAsync();
        }
        public List<Stomatolog> GetBySpecijalizacija(Specijalizacija specijalizacija)
        {
            return _stomatolozi.Find(d => d.Specijalizacija == specijalizacija).ToList();
        }
        public Stomatolog GetAndUpdate(ObjectId stomatologId, ObjectId pregledId)
        {
            return _stomatolozi.FindOneAndUpdate(
            Builders<Stomatolog>.Filter.Eq(d => d.Id, stomatologId),
            Builders<Stomatolog>.Update.AddToSet(d => d.PredstojeciPregledi, pregledId));
        }
        public Stomatolog GetAndAddReview(ObjectId stomatologId, ObjectId pregledId)
        {
            return _stomatolozi.FindOneAndUpdate(
            Builders<Stomatolog>.Filter.Eq(d => d.Id, stomatologId),
            Builders<Stomatolog>.Update.AddToSet(d => d.KomentariStomatologa, pregledId));
        }

        public void Remove(ObjectId id)
        {
            _stomatolozi.DeleteOne(stomatolog => stomatolog.Id == id);
        }

        public void Update(ObjectId id, Stomatolog stomatolog)
        {
            _stomatolozi.ReplaceOne(stomatolog => stomatolog.Id == id, stomatolog);
        }
        public async Task<bool> Register(RegistrationModelStomatolog resource)
        {
            var postojeciStomatolog = await _stomatolozi.Find(x => x.Email == resource.Email).FirstOrDefaultAsync();
            if (postojeciStomatolog != null)
            {
                throw new InvalidOperationException("Stomatolog sa ovom email adresom već postoji.");
            }

            var stomatolog = new Stomatolog
            {
                Ime = resource.Ime,
                Prezime = resource.Prezime,
                Adresa = resource.Adresa,
                BrojTelefona = resource.BrojTelefona,
                Role = UserRole.Stomatolog,
                Specijalizacija = resource.Specijalizacija,
                Email = resource.Email,
                PasswordSalt = PasswordHasher.GenerateSalt(),
            };

            stomatolog.Password = PasswordHasher.ComputeHash(resource.Password, stomatolog.PasswordSalt, _pepper, _iteration);
            await _stomatolozi.InsertOneAsync(stomatolog);

            return true;
        }

        public bool RemoveAppointment(ObjectId stomatologId, ObjectId appointmentId)
        {
            var filter = Builders<Stomatolog>.Filter.Eq(p => p.Id, stomatologId);
            var update = Builders<Stomatolog>.Update.Pull(p => p.PredstojeciPregledi, appointmentId);

            var result = _stomatolozi.UpdateOne(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}