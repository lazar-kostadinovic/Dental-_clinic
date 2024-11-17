using System.ComponentModel.DataAnnotations;

namespace StomatoloskaOrdinacija.Models
{

    public class RegistrationModelStomatolog
    {
        [Required]
        public string Ime { get; set; }
        [Required]
        public string Prezime { get; set; }
        [Required]
        public string Adresa { get; set; }
        [Required]
        public string BrojTelefona { get; set; }
        [Required]
        public Specijalizacija Specijalizacija { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
