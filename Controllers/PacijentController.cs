using StomatoloskaOrdinacija.Models;
using StomatoloskaOrdinacija.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StomatoloskaOrdinacija.DTOs;

namespace StomatoloskaOrdinacija.Controllers;

[ApiController]
[Route("[controller]")]
public class PacijentController : ControllerBase
{
    private readonly IPacijentService pacijentService;
    private readonly string _pepper;
    private readonly int _iteration;

    public PacijentController(IPacijentService pacijentService, IConfiguration config)
    {
        this.pacijentService = pacijentService;
        _pepper = config["PasswordHasher:Pepper"];
        _iteration = config.GetValue<int>("PasswordHasher:Iteration");
    }

    [HttpGet]
    public ActionResult<List<Pacijent>> Get()
    {
        return pacijentService.Get();
    }

    [HttpGet("{id}")]
    public ActionResult<Pacijent> Get(ObjectId id)
    {
        var pacijent = pacijentService.Get(id);

        if (pacijent == null)
        {
            return NotFound($"Pacijent with Id = {id} not found");
        }

        return pacijent;
    }
    [HttpGet("getDTO/{id}")]
    public ActionResult<PacijentDTO> Get(string id)
    {

        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest("Invalid ObjectId format.");
        }

        var pacijent = pacijentService.Get(objectId);

        if (pacijent == null)
        {
            return NotFound($"Pacijent with Id = {id} not found");
        }

        var pacijentDTO = new PacijentDTO
        {
            Id = pacijent.Id.ToString(),
            Slika = pacijent.Slika,
            Ime = pacijent.Ime,
            Prezime = pacijent.Prezime,
            Adresa = pacijent.Adresa,
            BrojTelefona = pacijent.BrojTelefona,
            Email = pacijent.Email,
            Role = pacijent.Role,
            IstorijaPregleda = pacijent.IstorijaPregleda.Select(id => id.ToString()).ToList()
        };
        return pacijentDTO;
    }
    [HttpGet("basic")]
    public IActionResult GetBasicPatientInfo()
    {
        var patients = pacijentService.Get()
        .Select(p=> new
        {
            Id=p.Id.ToString(),
            Name = $"{p.Ime} {p.Prezime}"
        }).ToList();
        return Ok(patients);
    }

    [HttpGet("GetPacijentByEmail/{email}")]
    public async Task<IActionResult> GetPacijentByEmail(string email)
    {
        var pacijent = await pacijentService.GetPacijentByEmailAsync(email);

        if (pacijent == null)
        {
            return NotFound($"Pacijent with email = {email} not found");
        }
        var pacijentDTO = new PacijentDTO
        {
            Id = pacijent.Id.ToString(),
            Slika = pacijent.Slika,
            Ime = pacijent.Ime,
            Prezime = pacijent.Prezime,
            Adresa = pacijent.Adresa,
            BrojTelefona = pacijent.BrojTelefona,
            Email = pacijent.Email,
            Role = pacijent.Role,
            IstorijaPregleda = pacijent.IstorijaPregleda.Select(id => id.ToString()).ToList()
        };
        return Ok(pacijentDTO);
    }

    [HttpPost("{ime}/{prezime}/{adresa}/{brojTelefona}/{email}")]
    public ActionResult<Pacijent> Post(string ime, string prezime, string adresa, string brojTelefona, string email)
    {
        var pacijent = new Pacijent
        {
            Ime = ime,
            Prezime = prezime,
            Adresa = adresa,
            BrojTelefona = brojTelefona,
            Email = email
        };
        pacijentService.Create(pacijent);

        return CreatedAtAction(nameof(Get), new { id = pacijent.Id }, pacijent);
    }

    [HttpPut("addOrUpdateSlika/{id}")]
    public IActionResult AddOrUpdateSlika(string id, [FromBody] string slikaFileName)
    {
        if (string.IsNullOrWhiteSpace(slikaFileName))
        {
            return BadRequest("Naziv fajla slike ne može biti prazan.");
        }

        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest("Invalid ObjectId format.");
        }

        var pacijent = pacijentService.Get(objectId);

        if (pacijent == null)
        {
            return NotFound($"Pacijent with Id = {id} not found");
        }

        var result = pacijentService.AddOrUpdateSlika(objectId, slikaFileName);

        if (!result)
        {
            return StatusCode(500, "Ažuriranje slike nije uspelo.");
        }

        return Ok($"Slika uspešno ažurirana za pacijenta sa Id = {id}.");
    }

    [HttpPost("uploadSlika/{id}")]
    public IActionResult UploadSlika(string id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var patient = pacijentService.Get(new ObjectId(id));
        if (patient == null)
        {
            return NotFound($"Pacijent with Id = {id} not found.");
        }
    
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine("wwwroot", "assets", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        patient.Slika = fileName;
        pacijentService.Update(patient.Id, patient);

        return Ok(new { fileName });
    }


    [HttpPut("{email}/{adresa}/{brojTelefona}/{newEmail}")]
    public async Task<ActionResult> Put(string email, string adresa, string brojTelefona, string newEmail)
    {
        var existingPacijent = await pacijentService.GetPacijentByEmailAsync(email);
        var id = existingPacijent.Id;
        if (existingPacijent == null)
        {
            return NotFound($"Pacijent with Email = {email} not found");
        }

        var pacijent = new Pacijent
        {
            Id = existingPacijent.Id,
            Ime = existingPacijent.Ime,
            Prezime = existingPacijent.Prezime,
            Adresa = adresa,
            BrojTelefona = brojTelefona,
            Email = newEmail,
            Password = existingPacijent.Password,
            PasswordSalt = existingPacijent.PasswordSalt,
            Role = existingPacijent.Role,
            IstorijaPregleda = existingPacijent.IstorijaPregleda
        };

        pacijentService.Update(id, pacijent);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(ObjectId id)
    {
        var pacijent = pacijentService.Get(id);

        if (pacijent == null)
        {
            return NotFound($"Pacijent with Id = {id} not found");
        }

        pacijentService.Remove(pacijent.Id);

        return Ok($"Pacijent with Id = {id} deleted");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationModel resource)
    {
        try
        {
            var response = await pacijentService.Register(resource);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = e.Message });
        }
    }

}