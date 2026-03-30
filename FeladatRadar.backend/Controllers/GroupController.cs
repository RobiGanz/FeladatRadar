using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException("Érvénytelen token.");
            return userId;
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";
        }

        /// <summary>Check if user can manage (add/edit/delete) in a group</summary>
        private async Task<bool> CanManageGroup(int groupId)
        {
            var userId = GetCurrentUserId();
            return await _groupService.CanManageGroupAsync(groupId, userId);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "A csoport neve kotelezo." });

            var result = await _groupService.CreateGroupAsync(GetCurrentUserId(), request.GroupName);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{groupId}/invite")]
        public async Task<IActionResult> Invite(int groupId, [FromBody] InviteToGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InvitedEmail))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Az email cim kotelezo." });

            var result = await _groupService.InviteToGroupAsync(groupId, GetCurrentUserId(), request.InvitedEmail);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("invite/{inviteId}/accept")]
        public async Task<IActionResult> AcceptInvite(int inviteId)
        {
            var result = await _groupService.AcceptInviteAsync(inviteId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("invite/{inviteId}/decline")]
        public async Task<IActionResult> DeclineInvite(int inviteId)
        {
            var result = await _groupService.DeclineInviteAsync(inviteId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{groupId}/leave")]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var result = await _groupService.LeaveGroupAsync(groupId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var groups = await _groupService.GetMyGroupsAsync(GetCurrentUserId());
            return Ok(groups);
        }

        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetMembers(int groupId)
        {
            var members = await _groupService.GetGroupMembersAsync(groupId, GetCurrentUserId());
            return Ok(members);
        }

        [HttpGet("{groupId}/subjects")]
        public async Task<IActionResult> GetSubjects(int groupId)
        {
            var subjects = await _groupService.GetGroupSubjectsAsync(groupId, GetCurrentUserId());
            return Ok(subjects);
        }

        [HttpGet("{groupId}/schedule")]
        public async Task<IActionResult> GetSchedule(int groupId)
        {
            var role = GetCurrentUserRole();
            var schedule = await _groupService.GetGroupScheduleAsync(groupId, GetCurrentUserId(), role);
            return Ok(schedule);
        }

        [HttpGet("{groupId}/tasks")]
        public async Task<IActionResult> GetTasks(int groupId)
        {
            var tasks = await _groupService.GetGroupTasksAsync(groupId, GetCurrentUserId());
            return Ok(tasks);
        }

        [HttpGet("my-invites")]
        public async Task<IActionResult> GetMyInvites()
        {
            var invites = await _groupService.GetMyInvitesAsync(GetCurrentUserEmail());
            return Ok(invites);
        }

        [HttpPost("{groupId}/schedule/add")]
        public async Task<IActionResult> AddGroupSchedule(int groupId, [FromBody] AddScheduleRequest request)
        {
            if (!await CanManageGroup(groupId))
                return StatusCode(403, new SubjectResponse { Status = "ERROR", Message = "Nincs jogosultságod a csoport kezeléséhez." });

            var result = await _groupService.AddGroupScheduleEntryAsync(groupId, GetCurrentUserId(), request, GetCurrentUserRole());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{groupId}/tasks/add")]
        public async Task<IActionResult> AddGroupTask(int groupId, [FromBody] AddGroupTaskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "A cím megadása kötelező." });

            if (!await CanManageGroup(groupId))
                return StatusCode(403, new SubjectResponse { Status = "ERROR", Message = "Nincs jogosultságod a csoport kezeléséhez." });

            var result = await _groupService.AddGroupTaskAsync(groupId, GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }

}
