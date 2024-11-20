using System.Globalization;
using StomatoloskaOrdinacija.Models;
using StomatoloskaOrdinacija.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StomatoloskaOrdinacija.DTOs;

namespace StomatoloskaOrdinacija.Controllers;

[ApiController]
[Route("[controller]")]
public class PregledController : ControllerBase
{
    private readonly IPregledService pregledService;
    private readonly IStomatologService stomatologService;
    private readonly IPacijentService pacijentService;


    public PregledController(IPregledService pregledService, IStomatologService stomatologService, IPacijentService pacijentService)
    {
        this.pregledService = pregledService;
        this.stomatologService = stomatologService;
        this.pacijentService = pacijentService;
    }

    [HttpGet]
    public ActionResult<List<Pregled>> Get()
    {
        return pregledService.Get();
    }

    [HttpGet("{id}")]
    public ActionResult<Pregled> Get(ObjectId id)
    {
        var pregled = pregledService.Get(id);

        if (pregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        return pregled;
    }

    [HttpGet("getPregledDTO/{id}")]
    public ActionResult<PregledDTO> GetPregledDTO(string id)
    {
        pregledService.UpdatePregledStatuses();
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return BadRequest("Invalid ObjectId format.");
        }

        var pregled = pregledService.Get(objectId);

        if (pregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        var pregledDTO = new PregledDTO
        {
            Id = pregled.Id.ToString(),
            IdStomatologa = pregled.IdStomatologa.ToString(),
            IdPacijenta = pregled.IdPacijenta.ToString(),
            Datum = pregled.Datum,
            Opis = pregled.Opis,
            Status = pregled.Status
        };
        return pregledDTO;
    }

    [HttpGet("availableTimeSlots/{idStomatologa}/{datum}")]
    public ActionResult<IEnumerable<string>> GetAvailableTimeSlots(ObjectId idStomatologa, DateTime datum)
    {
        try
        {
            var stomatolog = stomatologService.Get(idStomatologa);

            if (stomatolog == null)
            {
                return NotFound($"Stomatolog with Id = {idStomatologa} not found");
            }
            var allTimeSlots = new List<string> { "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00" };

            var existingAppointments = pregledService.GetStomatologAppointmentsInDateRange(idStomatologa, datum, datum.AddDays(1));
            // Console.WriteLine("Existing Appointments:");
            // foreach (var appointment in existingAppointments)
            // {
            //     Console.WriteLine($"Datum: {appointment.Datum}, Opis: {appointment.Opis}"); 
            // }

            var availableTimeSlots = allTimeSlots.Except(existingAppointments.Select(appointment => appointment.Datum.ToUniversalTime().ToString("HH:mm")));
            // var availableTimeSlots = allTimeSlots.Except(existingAppointments.Select(appointment => appointment.Datum.ToString("HH:mm")));

            return Ok(availableTimeSlots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost]
    public ActionResult<Pregled> Post([FromBody] Pregled pregled)
    {
        pregledService.Create(pregled);

        return CreatedAtAction(nameof(Get), new { id = pregled.Id }, pregled);
    }
    [HttpPost("schedule/{idStomatologa}/{idPacijenta}/{datum}/{opis}")]
    public IActionResult ScheduleAppointment(ObjectId idStomatologa, ObjectId idPacijenta, DateTime datum, string opis)
    {
        var pregled = new Pregled
        {
            IdStomatologa = idStomatologa,
            IdPacijenta = idPacijenta,
            Datum = datum.ToLocalTime(),
            Opis = opis,
            Status = StatusPregleda.Predstojeci
        };

        if (pregled == null)
        {
            return BadRequest("Invalid appointment data");
        }


        var stomatolog = stomatologService.Get(pregled.IdStomatologa);
        if (stomatolog == null)
        {
            return NotFound("Stomatolog not found");
        }

        var patient = pacijentService.Get(pregled.IdPacijenta);
        if (patient == null)
        {
            return NotFound("Patient not found");
        }

        if (IsAppointmentConflict(pregled))
        {
            return BadRequest("Appointment time conflict");
        }

        pregledService.Create(pregled);
        pacijentService.GetAndUpdate(pregled.IdPacijenta, pregled.Id);
        stomatologService.GetAndUpdate(pregled.IdStomatologa, pregled.Id);

        return CreatedAtAction(nameof(pregledService.Get), new { id = pregled.Id }, pregled);
    }

    [HttpPut("{id}/{datum}/{opis}")]
    public ActionResult Put(ObjectId id, DateTime datum, string opis)
    {
        var existingPregled = pregledService.Get(id);

        if (existingPregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        var pregled = new Pregled
        {
            Id = existingPregled.Id,
            IdStomatologa = existingPregled.IdStomatologa,
            IdPacijenta = existingPregled.IdPacijenta,
            Datum = datum,
            Opis = opis,
            Status = existingPregled.Status
        };

        pregledService.Update(id, pregled);

        return NoContent();
    }

    [HttpPut("{id}/{opis}")]
    public ActionResult Put(ObjectId id, string opis)
    {
        var existingPregled = pregledService.Get(id);

        if (existingPregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        var pregled = new Pregled
        {
            Id = existingPregled.Id,
            IdStomatologa = existingPregled.IdStomatologa,
            IdPacijenta = existingPregled.IdPacijenta,
            Datum = existingPregled.Datum,
            Opis = opis,
            Status = existingPregled.Status
        };

        pregledService.Update(id, pregled);

        return NoContent();
    }

    [HttpPut("updateStatus/{id}/{status}")]
    public ActionResult updateStatus(ObjectId id, int status)
    {
        var existingPregled = pregledService.Get(id);

        if (existingPregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        var pregled = new Pregled
        {
            Id = existingPregled.Id,
            IdStomatologa = existingPregled.IdStomatologa,
            IdPacijenta = existingPregled.IdPacijenta,
            Datum = existingPregled.Datum,
            Opis = existingPregled.Opis,
            Status = (StatusPregleda)status
        };

        pregledService.Update(id, pregled);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(ObjectId id)
    {
        var pregled = pregledService.Get(id);

        if (pregled == null)
        {
            return NotFound($"Pregled with Id = {id} not found");
        }

        var patientId = pregled.IdPacijenta;
        var stomatologId = pregled.IdStomatologa;

        if (!pacijentService.RemoveAppointment(patientId, id))
        {
            return NotFound($"Patient with Id = {patientId} or appointment with Id = {id} not found");
        }

        if (!stomatologService.RemoveAppointment(stomatologId, id))
        {
            return NotFound($"Stomatolog with Id = {stomatologId} or appointment with Id = {id} not found");
        }

        pregledService.Remove(id);


        return Ok(new { message = $"Pregled with Id = {id} deleted" });

    }

    private bool IsAppointmentConflict(Pregled pregled)
    {
        var stomatolog = stomatologService.Get(pregled.IdStomatologa);
        if (stomatolog.PredstojeciPregledi.Any(existingAppointmentId =>
            pregledService.Get(existingAppointmentId)?.Datum == pregled.Datum))
        {
            return true;
        }

        var patient = pacijentService.Get(pregled.IdPacijenta);
        if (patient.IstorijaPregleda.Any(existingAppointmentId =>
            pregledService.Get(existingAppointmentId)?.Datum == pregled.Datum))
        {
            return true;
        }

        return false;
    }
}