namespace StomatoloskaOrdinacija.Models
{
    public class OrdinacijaDatabaseSettings : IOrdinacijaDatabaseSettings
    {
        public string PregledCollectionName { get; set; } = String.Empty;
        public string StomatologCollectionName { get; set; } = String.Empty;
        public string PacijentCollectionName { get; set; } = String.Empty;
        public string OcenaStomatologaCollectionName { get; set; } = String.Empty;
        public string ConnectionString { get; set; } = String.Empty;
        public string DatabaseName { get; set; } = String.Empty;
    }
}