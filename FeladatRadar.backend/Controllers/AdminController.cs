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
            return int.Parse(claim ?? "0");
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
                $"TargetUserID={request.TargetUserID}, NewRole={request.NewRole}");

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
                $"TargetUserID={request.TargetUserID}, Name={request.FirstName} {request.LastName}");

            return Ok(result);
        }
        [HttpPost("users/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromBody] ToggleUserActiveRequest request)
        {
            var result = await _adminService.ToggleUserActiveAsync(GetCurrentUserId(), request.TargetUserID, request.IsActive);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "ToggleUserActive",
                $"TargetUserID={request.TargetUserID}, IsActive={request.IsActive}");

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUserAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "DeleteUser",
                $"TargetUserID={id}");

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
                $"GroupID={id}");

            return Ok(result);
        }

        [HttpDelete("polls/{id}")]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var result = await _adminService.AdminDeletePollAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeletePoll",
                $"PollID={id}");

            return Ok(result);
        }

        [HttpDelete("exams/{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var result = await _adminService.AdminDeleteExamAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteExam",
                $"ExamID={id}");

            return Ok(result);
        }

        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _adminService.AdminDeleteTaskAsync(GetCurrentUserId(), id);
            if (result.Status == "ERROR") return BadRequest(result);

            await _adminService.WriteAuditLogAsync(GetCurrentUserId(), "AdminDeleteTask",
                $"TaskID={id}");

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