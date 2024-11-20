using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public interface IOcenaStomatologaService
    {
        List<OcenaStomatologa> Get();
        OcenaStomatologa Get(ObjectId id);
        OcenaStomatologa Create(OcenaStomatologa ocena);
        void Update(ObjectId id, OcenaStomatologa ocena);
        void Remove(ObjectId id);
        List<OcenaStomatologa> GetReviews(ObjectId idStomatologa);
    }
}