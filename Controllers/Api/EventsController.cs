using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers.Api
{
    [ApiController]
    [Route("api/events")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public EventsController(ApplicationDbContext db) => _db = db;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static readonly Expression<Func<Event, EventResponse>> Map = e =>
            new EventResponse(e.Id, e.Name, e.Description, e.Date, e.Location, e.DistanceKm,
                e.EventType, e.OrganiserId,
                e.Categories.Select(c => new CategoryResponse(c.Id, c.Name)).ToList());

        private static readonly Func<Event, EventResponse> ToResponse = Map.Compile();

        /// <summary>Lists all events with their categories. Both roles.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Events.AsNoTracking().OrderBy(e => e.Date).Select(Map).ToListAsync());

        /// <summary>Gets one event by id. Both roles.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _db.Events.AsNoTracking().Where(e => e.Id == id).Select(Map).FirstOrDefaultAsync();
            return ev == null ? NotFound() : Ok(ev);
        }

        /// <summary>Creates an event. Organisers only.</summary>
        /// <remarks>Expects name, description, date, location, distanceKm and eventType (Run, Walk or Cycle).</remarks>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(EventDto dto)
        {
            var ev = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                OrganiserId = UserId
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ToResponse(ev));
        }

        /// <summary>Updates one of your events. Organisers only.</summary>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            ev.Name = dto.Name; ev.Description = dto.Description; ev.Date = dto.Date;
            ev.Location = dto.Location; ev.DistanceKm = dto.DistanceKm; ev.EventType = dto.EventType;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes one of your events. Organisers only.</summary>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}