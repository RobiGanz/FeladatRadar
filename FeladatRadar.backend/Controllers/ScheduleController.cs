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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(claim ?? "0");
        }

        [HttpGet("my-schedule")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMySchedule()
        {
            var entries = await _scheduleService.GetMyScheduleAsync(GetCurrentUserId());
            return Ok(entries);
        }

        [HttpPost("add")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddEntry([FromBody] AddScheduleRequest request)
        {
            var result = await _scheduleService.AddScheduleEntryAsync(GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete/{entryId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteEntry(int entryId)
        {
            var result = await _scheduleService.DeleteScheduleEntryAsync(entryId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }
}
