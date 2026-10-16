using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers.Api
{
    [ApiController]
    [Route("api/events")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public EventsController(ApplicationDbContext db) => _db = db;

        private const string Scheme = JwtBearerDefaults.AuthenticationScheme;

        /// <summary>Lists all events. Any authenticated user.</summary>
        /// <returns>A list of events.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Events.AsNoTracking().OrderBy(e => e.Date).ToListAsync());

        /// <summary>Gets one event by id. Any authenticated user.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            return ev == null ? NotFound() : Ok(ev);
        }

        /// <summary>Creates an event. Organisers only.</summary>
        /// <remarks>Expects name, location and date. Description is optional.</remarks>
        [HttpPost]
        [Authorize(AuthenticationSchemes = Scheme, Roles = "Organiser")]
        [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(EventDto dto)
        {
            var ev = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = dto.Location,
                Date = dto.Date,
                OrganiserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
        }

        /// <summary>Updates an event. Organisers only.</summary>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = Scheme, Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.Location = dto.Location;
            ev.Date = dto.Date;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes an event. Organisers only.</summary>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = Scheme, Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Enrols the current Participant in an event.</summary>
        /// <remarks>No body needed. The participant is taken from the token.</remarks>
        [HttpPost("{id}/enrol")]
        [Authorize(AuthenticationSchemes = Scheme, Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int id)
        {
            if (!await _db.Events.AnyAsync(e => e.Id == id)) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (await _db.Enrolments.AnyAsync(e => e.EventId == id && e.UserId == userId))
                return Conflict("Already enrolled in this event.");

            _db.Enrolments.Add(new Enrolment { EventId = id, UserId = userId });
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}