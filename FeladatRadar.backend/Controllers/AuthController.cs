using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Új felhasználó regisztrálása.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
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
                    Message = errors.Count > 0 ? string.Join(" | ", errors) : "Érvénytelen adatok."
                });
            }
            var response = await _authService.Register(request);
            if (response.Status == "ERROR")
            {
                bool isDuplicate = response.Message.Contains("már létezik", StringComparison.OrdinalIgnoreCase)
                    || response.Message.Contains("already", StringComparison.OrdinalIgnoreCase)
                    || response.Message.Contains("foglalt", StringComparison.OrdinalIgnoreCase);
                return isDuplicate ? Conflict(response) : BadRequest(response);
            }
            return Ok(response);
        }

        /// <summary>Bejelentkezés, JWT token visszaadása.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _authService.Login(request);
            if (response.Status == "ERROR") return Unauthorized(response);
            return Ok(response);
        }

        /// <summary>A bejelentkezett felhasználó adatainak lekérdezése.</summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _authService.GetUserById(GetCurrentUserId());
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

        /// <summary>Felhasználónév módosítása.</summary>
        [Authorize]
        [HttpPut("update-username")]
        public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
        {
            var response = await _authService.UpdateUsername(GetCurrentUserId(), request.NewUsername);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }

        /// <summary>Jelszó módosítása.</summary>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var response = await _authService.ChangePassword(GetCurrentUserId(), request.CurrentPassword, request.NewPassword);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }
    }
}
