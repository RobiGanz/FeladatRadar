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
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException("Érvénytelen token.");
            return userId;
        }

        [HttpGet("my-tasks")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> GetMyTasks()
        {
            var tasks = await _taskService.GetMyTasksAsync(GetCurrentUserId());
            return Ok(tasks);
        }

        [HttpPost("add")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> AddTask([FromBody] AddTaskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "A cím megadása kötelező." });

            var result = await _taskService.AddTaskAsync(GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("complete/{taskId}")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var result = await _taskService.CompleteTaskAsync(taskId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("uncomplete/{taskId}")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> UncompleteTask(int taskId)
        {
            var result = await _taskService.UncompleteTaskAsync(taskId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete/{taskId}")]
        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var result = await _taskService.DeleteTaskAsync(taskId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }
}
