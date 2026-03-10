using FeladatRadar.backend.Models;
using FeladatRadar.frontend.Services;
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
            return int.Parse(claim ?? "0");
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Student";
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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddGroupSchedule(int groupId, [FromBody] AddScheduleRequest request)
        {
            request.DayOfWeek = request.DayOfWeek;
            var result = await _groupService.AddGroupScheduleEntryAsync(groupId, GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }

}
