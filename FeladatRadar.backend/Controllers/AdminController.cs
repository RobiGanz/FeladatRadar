using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException("Érvénytelen token.");
            return userId;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            var stats = await _adminService.GetSystemStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("users/change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewRole))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Az új szerepkör megadása kötelező." });

            var result = await _adminService.ChangeUserRoleAsync(GetCurrentUserId(), request.TargetUserID, request.NewRole);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "ChangeUserRole",
                $"Felhasználó (ID:{request.TargetUserID}) szerepköre módosítva → {request.NewRole}");

            return Ok(result);
        }
        [HttpPost("users/rename")]
        public async Task<IActionResult> RenameUser([FromBody] RenameUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Név megadása kötelező." });

            var result = await _adminService.RenameUserAsync(GetCurrentUserId(), request.TargetUserID, request.FirstName, request.LastName);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "RenameUser",
                $"Felhasználó (ID:{request.TargetUserID}) neve módosítva → {request.FirstName} {request.LastName}");

            return Ok(result);
        }
        [HttpPost("users/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromBody] ToggleUserActiveRequest request)
        {
            var result = await _adminService.ToggleUserActiveAsync(GetCurrentUserId(), request.TargetUserID, request.IsActive);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "ToggleUserActive",
                $"Felhasználó (ID:{request.TargetUserID}) {(request.IsActive ? "aktiválva" : "letiltva")}");

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUserAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "DeleteUser",
                $"Felhasználó törölve (ID:{id})");

            return Ok(result);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _adminService.GetAllGroupsAsync();
            return Ok(groups);
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var result = await _adminService.DeleteGroupAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "DeleteGroup",
                $"Csoport törölve (ID:{id})");

            return Ok(result);
        }

        [HttpDelete("polls/{id}")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var result = await _adminService.AdminDeletePollAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeletePoll",
                $"Szavazás törölve (ID:{id})");

            return Ok(result);
        }

        [HttpDelete("exams/{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var result = await _adminService.AdminDeleteExamAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteExam",
                $"Dolgozat törölve (ID:{id})");

            return Ok(result);
        }

        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _adminService.AdminDeleteTaskAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteTask",
                $"Feladat törölve (ID:{id})");

            return Ok(result);
        }

        [HttpGet("audit-log")]
        public async Task<IActionResult> GetAuditLog([FromQuery] int limit = 100)
        {
            var log = await _adminService.GetAuditLogAsync(limit);
            return Ok(log);
        }
    }
}