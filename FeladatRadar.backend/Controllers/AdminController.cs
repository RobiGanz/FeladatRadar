using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>Rendszerszintű statisztikák lekérdezése.</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            var stats = await _adminService.GetSystemStatsAsync();
            return Ok(stats);
        }

        /// <summary>Az összes felhasználó listázása.</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>Felhasználó szerepkörének módosítása.</summary>
        [HttpPost("users/change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewRole))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Az új szerepkör megadása kötelező." });
            var result = await _adminService.ChangeUserRoleAsync(GetCurrentUserId(), request.TargetUserID, request.NewRole);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "ChangeUserRole",
                $"Felhasználó (ID:{request.TargetUserID}) szerepköre módosítva: {request.NewRole}");
            return Ok(result);
        }

        /// <summary>Felhasználó nevének módosítása.</summary>
        [HttpPost("users/rename")]
        public async Task<IActionResult> RenameUser([FromBody] RenameUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Név megadása kötelező." });
            var result = await _adminService.RenameUserAsync(GetCurrentUserId(), request.TargetUserID, request.FirstName, request.LastName);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "RenameUser",
                $"Felhasználó (ID:{request.TargetUserID}) neve módosítva: {request.FirstName} {request.LastName}");
            return Ok(result);
        }

        /// <summary>Felhasználó aktiválása / letiltása.</summary>
        [HttpPost("users/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromBody] ToggleUserActiveRequest request)
        {
            var result = await _adminService.ToggleUserActiveAsync(GetCurrentUserId(), request.TargetUserID, request.IsActive);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "ToggleUserActive",
                $"Felhasználó (ID:{request.TargetUserID}) {(request.IsActive ? "aktiválva" : "letiltva")}");
            return Ok(result);
        }

        /// <summary>Felhasználó törlése.</summary>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUserAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "DeleteUser", $"Felhasználó törölve (ID:{id})");
            return Ok(result);
        }

        /// <summary>Az összes csoport listázása.</summary>
        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _adminService.GetAllGroupsAsync();
            return Ok(groups);
        }

        /// <summary>Csoport törlése adminként.</summary>
        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var result = await _adminService.DeleteGroupAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "DeleteGroup", $"Csoport törölve (ID:{id})");
            return Ok(result);
        }

        /// <summary>Egy csoport dolgozatainak listázása.</summary>
        [HttpGet("groups/{id}/exams")]
        public async Task<IActionResult> GetGroupExams(int id)
        {
            var exams = await _adminService.GetGroupExamsAsync(id);
            return Ok(exams);
        }

        /// <summary>Szavazás törlése adminként.</summary>
        [HttpDelete("polls/{id}")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var result = await _adminService.AdminDeletePollAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeletePoll", $"Szavazás törölve (ID:{id})");
            return Ok(result);
        }

        /// <summary>Dolgozat törlése adminként.</summary>
        [HttpDelete("exams/{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var result = await _adminService.AdminDeleteExamAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteExam", $"Dolgozat törölve (ID:{id})");
            return Ok(result);
        }

        /// <summary>Feladat törlése adminként.</summary>
        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _adminService.AdminDeleteTaskAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);
            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteTask", $"Feladat törölve (ID:{id})");
            return Ok(result);
        }

        /// <summary>Audit napló lekérdezése.</summary>
        [HttpGet("audit-log")]
        public async Task<IActionResult> GetAuditLog([FromQuery] int limit = 100)
        {
            var log = await _adminService.GetAuditLogAsync(limit);
            return Ok(log);
        }
    }
}
