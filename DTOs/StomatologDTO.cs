using StomatoloskaOrdinacija.Models;

namespace StomatoloskaOrdinacija.DTOs
{
    public class StomatologDTO
    {
        public string Id { get; set; }
        public string Slika { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public string Email { get; set; }
        public string BrojTelefona { get; set; }
        public UserRole Role { get; set; }
        public Specijalizacija Specijalizacija { get; set; }
        public List<string> PredstojeciPregledi { get; set; }
        public List<string> KomentariStomatologa { get; set; }
    }
}
