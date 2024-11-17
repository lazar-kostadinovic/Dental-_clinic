namespace StomatoloskaOrdinacija.DTOs
{
    public class OcenaStomatologaDTO
    {
        public string Id { get; set; }
        public string IdStomatologa { get; set; }
        public string IdPacijenta { get; set; }
        public DateTime Datum { get; set; }
        public string Komentar { get; set; }
        public int Ocena { get; set; }
    }
}
