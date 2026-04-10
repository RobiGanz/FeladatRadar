using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Teacher")]
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>A bejelentkezett felhasználó órarendjének lekérdezése.</summary>
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            var entries = await _scheduleService.GetMyScheduleAsync(GetCurrentUserId());
            return Ok(entries);
        }

        /// <summary>Új órarend bejegyzés hozzáadása.</summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddEntry([FromBody] AddScheduleRequest request)
        {
            var result = await _scheduleService.AddScheduleEntryAsync(GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Órarend bejegyzés törlése.</summary>
        [HttpDelete("delete/{entryId}")]
        public async Task<IActionResult> DeleteEntry(int entryId)
        {
            var result = await _scheduleService.DeleteScheduleEntryAsync(entryId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }
}
