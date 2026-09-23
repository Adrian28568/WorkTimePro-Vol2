using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTimePro.Api.Data;
using WorkTimePro.Api.Models;

namespace WorkTimePro.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password are required");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized("Invalid credentials");

            bool isValid;

            if (IsBcryptHash(user.PasswordHash))
            {
                // Normal case: real BCrypt hash
                isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            else
            {
                // Leftover plaintext row from tonight — compare directly,
                // then silently upgrade it to a real BCrypt hash so this only happens once
                isValid = user.PasswordHash == request.Password;
                if (isValid)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    await _context.SaveChangesAsync();
                }
            }

            if (!isValid)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.IsAdmin
            });
        }

        private static bool IsBcryptHash(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   (value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$"));
        }
    }
}