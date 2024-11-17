namespace StomatoloskaOrdinacija.Models
{
    public interface IOrdinacijaDatabaseSettings
    {
        string StomatologCollectionName { get; set; }
        string PacijentCollectionName { get; set; }
        string PregledCollectionName { get; set; }
        string OcenaStomatologaCollectionName{get;set;}
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}