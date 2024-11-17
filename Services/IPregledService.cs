using StomatoloskaOrdinacija.Models;
using MongoDB.Bson;

namespace StomatoloskaOrdinacija.Services
{
    public interface IPregledService
    {
        List<Pregled> Get();
        Pregled Get(ObjectId id);
        Pregled GetByTimestamp(long timestamp);
        Pregled Create(Pregled pregled);
        void Update(ObjectId id, Pregled pregled);
        void UpdatePregledStatuses();
        void Remove(ObjectId id);
        List<Pregled> GetStomatologAppointmentsInDateRange(ObjectId idStomatologa, DateTime startDate, DateTime endDate);
    }
}