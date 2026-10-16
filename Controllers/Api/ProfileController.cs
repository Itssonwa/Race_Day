using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers.Api
{
    [ApiController]
    [Route("api/profile")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        public ProfileController(UserManager<ApplicationUser> users) => _users = users;

        private Task<ApplicationUser?> Current() =>
            _users.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Gets the logged-in user's own profile. Both roles.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            var u = await Current();
            if (u == null) return Unauthorized();
            return Ok(new ProfileResponse(u.Id, u.Email!, u.FirstName, u.LastName,
                u.Gender, u.PhoneNumber, await _users.GetRolesAsync(u)));
        }

        /// <summary>Updates the logged-in user's own profile. Both roles.</summary>
        /// <remarks>Expects first name, last name, gender and an optional phone number.</remarks>
        [HttpPut]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(UpdateProfileDto dto)
        {
            var u = await Current();
            if (u == null) return Unauthorized();

            u.FirstName = dto.FirstName;
            u.LastName = dto.LastName;
            u.Gender = dto.Gender;
            u.PhoneNumber = dto.PhoneNumber;

            var result = await _users.UpdateAsync(u);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));

            return Ok(new ProfileResponse(u.Id, u.Email!, u.FirstName, u.LastName,
                u.Gender, u.PhoneNumber, await _users.GetRolesAsync(u)));
        }
    }
}
