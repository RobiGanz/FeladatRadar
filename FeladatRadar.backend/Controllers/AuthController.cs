using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // [ApiController] már automatikusan ellenőrzi a ModelState-et,
            // de itt explicit kezelés az egyedi hibaüzenetekhez:
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
                return BadRequest(new LoginResponse
                {
                    Status = "ERROR",
                    Message = errors.Count > 0
                        ? string.Join(" | ", errors)
                        : "Érvénytelen adatok."
                });
            }

            var response = await _authService.Register(request);

            if (response.Status == "ERROR")
            {
                // Duplikált felhasználónév / email → 409 Conflict
                // Egyéb backend hiba → 400 Bad Request
                bool isDuplicate = response.Message.Contains("már létezik", StringComparison.OrdinalIgnoreCase)
                    || response.Message.Contains("already", StringComparison.OrdinalIgnoreCase)
                    || response.Message.Contains("foglalt", StringComparison.OrdinalIgnoreCase);
                return isDuplicate ? Conflict(response) : BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _authService.Login(request);
            if (response.Status == "ERROR") return Unauthorized(response);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Érvénytelen token." });

            var user = await _authService.GetUserById(userId);
            if (user == null)
                return NotFound(new { message = "A felhasználó nem található." });

            return Ok(new UserDto
            {
                UserID = user.UserID,
                Username = user.Username,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                UserRole = user.UserRole
            });
        }

        [Authorize]
        [HttpPut("update-username")]
        public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();
            var response = await _authService.UpdateUsername(userId, request.NewUsername);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();
            var response = await _authService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }
    }
}
