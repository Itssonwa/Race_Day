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
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ResultsController(ApplicationDbContext db) => _db = db;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static readonly Expression<Func<Result, ResultResponse>> Map = r =>
            new ResultResponse(r.Id, r.EnrolmentId, r.Enrolment!.EventId, r.Enrolment.Event!.Name,
                r.Enrolment.User!.FirstName + " " + r.Enrolment.User.LastName,
                r.Enrolment.Category!.Name, r.FinishTime, r.Position);

        /// <summary>Records a participant's finish time and position. Organisers only.</summary>
        /// <remarks>Expects an enrolmentId for this event, a finishTime ("hh:mm:ss") and a position of 1 or more.</remarks>
        [HttpPost("api/events/{eventId}/results")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(int eventId, ResultDto dto)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            if (!await _db.Enrolments.AnyAsync(e => e.Id == dto.EnrolmentId && e.EventId == eventId))
                return NotFound("Enrolment not found for this event.");
            if (await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
                return Conflict("A result already exists for this enrolment. Use PUT to change it.");

            var result = new Result { EnrolmentId = dto.EnrolmentId, FinishTime = dto.FinishTime, Position = dto.Position };
            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            var response = await _db.Results.AsNoTracking().Where(r => r.Id == result.Id).Select(Map).FirstAsync();
            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>Corrects an existing result. Organisers only.</summary>
        [HttpPut("api/events/{eventId}/results/{resultId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int eventId, int resultId, UpdateResultDto dto)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            var result = await _db.Results.Include(r => r.Enrolment)
                .FirstOrDefaultAsync(r => r.Id == resultId && r.Enrolment!.EventId == eventId);
            if (result == null) return NotFound();

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Lists all results for one of your events. Organisers only.</summary>
        [HttpGet("api/events/{eventId}/results")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            return Ok(await _db.Results.AsNoTracking()
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.Position).Select(Map).ToListAsync());
        }

        /// <summary>Lists the logged-in Participant's own results.</summary>
        [HttpGet("api/results/mine")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Participant")]
        [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Mine() =>
            Ok(await _db.Results.AsNoTracking()
                .Where(r => r.Enrolment!.UserId == UserId).Select(Map).ToListAsync());
    }
}
