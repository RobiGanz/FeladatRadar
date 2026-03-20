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
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(claim ?? "0");
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSubjects()
        {
            int studentId = GetCurrentUserId();
            var subjects = await _subjectService.GetAvailableSubjectsAsync(studentId);
            return Ok(subjects);
        }

        [HttpGet("my-enrollments")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            int studentId = GetCurrentUserId();
            var enrollments = await _subjectService.GetMyEnrollmentsAsync(studentId);
            return Ok(enrollments);
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int studentId = GetCurrentUserId();
            var response = await _subjectService.EnrollAsync(studentId, request.SubjectID);

            if (response.Status == "ERROR")
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("drop/{subjectId}")]
        public async Task<IActionResult> Drop(int subjectId)
        {
            int studentId = GetCurrentUserId();
            var response = await _subjectService.DropAsync(studentId, subjectId);

            if (response.Status == "ERROR")
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("enroll-custom")]
        public async Task<IActionResult> EnrollCustom([FromBody] CustomEnrollRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int studentId = GetCurrentUserId();
            var response = await _subjectService.EnrollCustomAsync(studentId, request);

            if (response.Status == "ERROR")
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return Ok(subjects);
        }
    }

}
