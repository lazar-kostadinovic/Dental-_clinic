using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public interface IStomatologService
    {
        List<Stomatolog> Get();
        Stomatolog Get(ObjectId id);
        Task<Stomatolog> GetStomatologByEmailAsync(string email);
        List<Stomatolog> GetBySpecijalizacija(Specijalizacija specijalizacija);
        Stomatolog GetAndUpdate(ObjectId stomatologId, ObjectId pregledId);
        Stomatolog GetAndAddReview(ObjectId stomatologId, ObjectId pregledId);
        Stomatolog Create(Stomatolog pacijent);
        void Update(ObjectId id, Stomatolog stomatolog);
        void Remove(ObjectId id);
        bool RemoveAppointment(ObjectId stomatologId, ObjectId appointmentId);
        Task<bool> Register(RegistrationModelStomatolog resource);
    }
}