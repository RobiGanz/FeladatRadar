using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubjectController : BaseController
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        /// <summary>Az összes felvehető (aktív, szabad helyes) tantárgy listázása.</summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSubjects()
        {
            var subjects = await _subjectService.GetAvailableSubjectsAsync(GetCurrentUserId());
            return Ok(subjects);
        }

        /// <summary>A bejelentkezett hallgató által már felvett tantárgyak listázása.</summary>
        [HttpGet("my-enrollments")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var enrollments = await _subjectService.GetMyEnrollmentsAsync(GetCurrentUserId());
            return Ok(enrollments);
        }

        /// <summary>Tantárgy felvétele a listából.</summary>
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _subjectService.EnrollAsync(GetCurrentUserId(), request.SubjectID);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }

        /// <summary>Tantárgy leadása.</summary>
        [HttpDelete("drop/{subjectId}")]
        public async Task<IActionResult> Drop(int subjectId)
        {
            var response = await _subjectService.DropAsync(GetCurrentUserId(), subjectId);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }

        /// <summary>Egyedi (listán nem szereplő) tantárgy felvétele.</summary>
        [HttpPost("enroll-custom")]
        public async Task<IActionResult> EnrollCustom([FromBody] CustomEnrollRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var response = await _subjectService.EnrollCustomAsync(GetCurrentUserId(), request);
            if (response.Status == "ERROR") return BadRequest(response);
            return Ok(response);
        }

        /// <summary>Az összes tantárgy listázása (tanároknak / adminnak).</summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return Ok(subjects);
        }
    }
}
