using StomatoloskaOrdinacija.Models;
using StomatoloskaOrdinacija.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StomatoloskaOrdinacija.DTOs;

namespace StomatoloskaOrdinacija.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class StomatologController : ControllerBase
{
    private readonly IStomatologService stomatologService;

    public StomatologController(IStomatologService stomatologService, IConfiguration config)
    {
        this.stomatologService = stomatologService;
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<List<Stomatolog>> Get()
    {
        return stomatologService.Get();
    }
    [AllowAnonymous]
    [HttpGet("getDTOs")]
    public ActionResult<StomatologDTO> GetStomatologDTOs()
    {
        var stomatolozi = stomatologService.Get();

        if (stomatolozi == null || !stomatolozi.Any())
        {
            return NotFound("Nema stomatologa u sistemu.");
        }

        var stomatoloziDTO = stomatolozi.Select(stomatolog => new StomatologDTO
        {
            Id = stomatolog.Id.ToString(),
            Slika = stomatolog.Slika,
            Ime = stomatolog.Ime,
            Prezime = stomatolog.Prezime,
            Adresa = stomatolog.Adresa,
            Email = stomatolog.Email,
            BrojTelefona = stomatolog.BrojTelefona,
            Role = stomatolog.Role,
            Specijalizacija = stomatolog.Specijalizacija,
            PredstojeciPregledi = stomatolog.PredstojeciPregledi.Select(p => p.ToString()).ToList(),
            KomentariStomatologa = stomatolog.KomentariStomatologa.Select(k => k.ToString()).ToList()
        }).ToList();

        return Ok(stomatoloziDTO);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public ActionResult<Stomatolog> Get(ObjectId id)
    {
        var stomatolog = stomatologService.Get(id);

        if (stomatolog == null)
        {
            return NotFound($"Stomatolog with Id = {id} not found");
        }

        return stomatolog;
    }

    [AllowAnonymous]
    [HttpGet("getStomatologDTO/{id}")]
    public ActionResult<StomatologDTO> GetStomatologById(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest("Invalid ObjectId format.");
        }

        var stomatolog = stomatologService.Get(objectId);

        if (stomatolog == null)
        {
            return NotFound($"Stomatolog with Id = {id} not found");
        }

        var stomatologDTO = new StomatologDTO
        {
            Id = stomatolog.Id.ToString(),
            Slika = stomatolog.Slika,
            Ime = stomatolog.Ime,
            Prezime = stomatolog.Prezime,
            Adresa = stomatolog.Adresa,
            Email = stomatolog.Email,
            BrojTelefona = stomatolog.BrojTelefona,
            Role = stomatolog.Role,
            Specijalizacija = stomatolog.Specijalizacija,
            PredstojeciPregledi = stomatolog.PredstojeciPregledi.Select(p => p.ToString()).ToList(),
            KomentariStomatologa = stomatolog.KomentariStomatologa.Select(k => k.ToString()).ToList()
        };

        return stomatologDTO;
    }



    [HttpGet("byTimestamp/{timestamp}")]
    public ActionResult<Stomatolog> GetByTimestamp(long timestamp)
    {
        var stomatolog = stomatologService.GetByTimestamp(timestamp);

        if (stomatolog == null)
        {
            return NotFound($"Stomatolog with timestamp = {timestamp} not found");
        }

        return stomatolog;
    }

    [AllowAnonymous]
    [HttpGet("GetStomatologByEmail/{email}")]
    public async Task<IActionResult> GetStomatologByEmail(string email)
    {
        var stomatolog = await stomatologService.GetStomatologByEmailAsync(email);

        if (stomatolog == null)
        {
            return NotFound($"Stomatolog with email = {email} not found");
        }
        var stomatologDTO = new StomatologDTO
        {
            Id = stomatolog.Id.ToString(),
            Slika = stomatolog.Slika,
            Ime = stomatolog.Ime,
            Prezime = stomatolog.Prezime,
            BrojTelefona = stomatolog.BrojTelefona,
            Email = stomatolog.Email,
            Role = stomatolog.Role,
            Specijalizacija = stomatolog.Specijalizacija,
            KomentariStomatologa = stomatolog.KomentariStomatologa.Select(id => id.ToString()).ToList(),
            PredstojeciPregledi = stomatolog.PredstojeciPregledi.Select(id => id.ToString()).ToList(),
        };

        return Ok(stomatologDTO);
    }

    [AllowAnonymous]
    [HttpGet("BySpecijalizacija/{specijalizacija}")]
    public ActionResult<List<Stomatolog>> GetBySpecijalizacija(Specijalizacija specijalizacija)
    {
        var stomatoloziBySpecijalizacija = stomatologService.GetBySpecijalizacija(specijalizacija);

        if (stomatoloziBySpecijalizacija.Count == 0)
        {
            return NotFound($"Stomatolog sa specijalizacijom = {specijalizacija} nije pronadjen");
        }

        return stomatoloziBySpecijalizacija;
    }


    [HttpPost("{ime}/{prezime}/{adresa}/{brojTelefona}/{email}/{specijalizacija}")]
    public ActionResult<Stomatolog> Post(string ime, string prezime, string adresa, string brojTelefona, string email, Specijalizacija specijalizacija)
    {
        var stomatolog = new Stomatolog
        {
            Ime = ime,
            Prezime = prezime,
            Adresa = adresa,
            BrojTelefona = brojTelefona,
            Email = email,
            Specijalizacija = specijalizacija
        };
        stomatologService.Create(stomatolog);

        return CreatedAtAction(nameof(Get), new { id = stomatolog.Id }, stomatolog);
    }

    [HttpPut("{email}/{adresa}/{brojTelefona}/{newEmail}/{specijalizacija}")]
    public async Task<ActionResult> Put(string email, string adresa, string brojTelefona, string newEmail, Specijalizacija specijalizacija)
    {
        var existingStomatolog = await stomatologService.GetStomatologByEmailAsync(email);
        var id = existingStomatolog.Id;

        if (existingStomatolog == null)
        {
            return NotFound($"Stomatolog sa Email = {email} nije pronadjen");
        }

        var stomatolog = new Stomatolog
        {
            Id = existingStomatolog.Id,
            Ime = existingStomatolog.Ime,
            Prezime = existingStomatolog.Prezime,
            Adresa = adresa,
            Email = newEmail,
            Password = existingStomatolog.Password,
            PasswordSalt = existingStomatolog.PasswordSalt,
            BrojTelefona = brojTelefona,
            Role = existingStomatolog.Role,
            Specijalizacija = specijalizacija,
            PredstojeciPregledi = existingStomatolog.PredstojeciPregledi
        };

        stomatologService.Update(id, stomatolog);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(ObjectId id)
    {
        var stomatolog = stomatologService.Get(id);

        if (stomatolog == null)
        {
            return NotFound($"Stomatolog with Id = {id} not found");
        }

        stomatologService.Remove(stomatolog.Id);

        return Ok($"Stomatolog with Id = {id} deleted");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationModelStomatolog resource)
    {
        try
        {
            var response = await stomatologService.Register(resource);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = e.Message });
        }
    }
}