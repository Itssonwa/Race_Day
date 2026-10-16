using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;

namespace RaceDay.Controllers.Api
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>Either "Participant" (default) or "Organiser".</summary>
        public string Role { get; set; } = "Participant";
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        /// <summary>Registers a new account as a Participant or an Organiser.</summary>
        /// <remarks>
        /// Expects email, password (must meet Identity rules), first name, last name and an
        /// optional role ("Participant" or "Organiser", default "Participant").
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
           
            if (dto.Role != "Participant" && dto.Role != "Organiser")
                return BadRequest("Role must be 'Participant' or 'Organiser'.");

       
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

        
            await _users.AddToRoleAsync(user, dto.Role);
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>Logs in and returns a JWT plus the user's roles.</summary>
        /// <remarks>Expects email and password. Use the token as a Bearer token in Swagger's Authorize button.</remarks>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null || !await _users.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            var roles = await _users.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                roles
            });
        }
    }
}