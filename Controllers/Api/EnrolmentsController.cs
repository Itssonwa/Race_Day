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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public EnrolmentsController(ApplicationDbContext db) => _db = db;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static readonly Expression<Func<Enrolment, EnrolmentResponse>> Map = e =>
            new EnrolmentResponse(e.Id, e.EventId, e.Event!.Name, e.CategoryId, e.Category!.Name,
                e.UserId, e.User!.FirstName + " " + e.User.LastName, e.EnrolledOn);

        /// <summary>Enrols the logged-in Participant in an event under a chosen category.</summary>
        /// <remarks>Expects a categoryId belonging to that event. The participant comes from the token.</remarks>
        [HttpPost("api/events/{eventId}/enrol")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Participant")]
        [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int eventId, EnrolDto dto)
        {
            if (!await _db.Events.AnyAsync(e => e.Id == eventId)) return NotFound("Event not found.");
            if (!await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.EventId == eventId))
                return BadRequest("That category does not belong to this event.");
            if (await _db.Enrolments.AnyAsync(e => e.EventId == eventId && e.UserId == UserId))
                return Conflict("Already enrolled in this event.");

            var enrolment = new Enrolment { EventId = eventId, CategoryId = dto.CategoryId, UserId = UserId };
            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return Ok(await _db.Enrolments.AsNoTracking().Where(e => e.Id == enrolment.Id).Select(Map).FirstAsync());
        }

        /// <summary>Lists all enrolments for one of your events. Organisers only.</summary>
        [HttpGet("api/events/{eventId}/enrolments")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            return Ok(await _db.Enrolments.AsNoTracking().Where(e => e.EventId == eventId).Select(Map).ToListAsync());
        }

        /// <summary>Lists the logged-in Participant's own enrolments.</summary>
        [HttpGet("api/enrolments/mine")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Participant")]
        [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Mine() =>
            Ok(await _db.Enrolments.AsNoTracking().Where(e => e.UserId == UserId).Select(Map).ToListAsync());
    }
}
