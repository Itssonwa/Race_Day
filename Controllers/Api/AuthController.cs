using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IConfiguration _config;
        public AuthController(UserManager<ApplicationUser> users, IConfiguration config)
        { _users = users; _config = config; }

        public record RegisterDto(string Email, string Password, string FirstName, string LastName);
        public record LoginDto(string Email, string Password);

        /// <summary>Registers a new account with the Participant role.</summary>
        /// <remarks>Expects email, password (meets Identity rules), first and last name.</remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));
            await _users.AddToRoleAsync(user, "Participant");
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>Logs in and returns a JWT to use as a Bearer token.</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null || !await _users.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, user.Id),
               new(ClaimTypes.Email, user.Email!)
           };
            claims.AddRange((await _users.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"], _config["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
