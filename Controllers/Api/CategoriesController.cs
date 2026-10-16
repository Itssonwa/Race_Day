using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers.Api
{
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db) => _db = db;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Lists the categories of an event (e.g. Under 20, Senior, 10km). Both roles.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.Id == eventId)) return NotFound();
            return Ok(await _db.Categories.AsNoTracking().Where(c => c.EventId == eventId)
                .Select(c => new CategoryResponse(c.Id, c.Name)).ToListAsync());
        }

        /// <summary>Adds a category to one of your events. Organisers only.</summary>
        /// <remarks>Expects a name, e.g. "Under 20" or "21km".</remarks>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(int eventId, CategoryDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);
            if (await _db.Categories.AnyAsync(c => c.EventId == eventId && c.Name == dto.Name))
                return Conflict("This event already has a category with that name.");

            var cat = new Category { EventId = eventId, Name = dto.Name };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new CategoryResponse(cat.Id, cat.Name));
        }

        /// <summary>Deletes a category from one of your events. Organisers only.</summary>
        /// <remarks>Fails with 409 if participants have already enrolled in it.</remarks>
        [HttpDelete("{categoryId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int eventId, int categoryId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != UserId) return StatusCode(StatusCodes.Status403Forbidden);

            var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.EventId == eventId);
            if (cat == null) return NotFound();
            if (await _db.Enrolments.AnyAsync(e => e.CategoryId == categoryId))
                return Conflict("Participants are enrolled in this category.");

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
