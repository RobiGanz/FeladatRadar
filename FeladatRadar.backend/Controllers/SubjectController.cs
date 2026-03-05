using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Minden endpoint JWT tokent vár
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // Bejelentkezett user ID-jának kinyerése a tokenből
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(claim ?? "0");
        }


        /// GET /api/subject/available
        /// Felvehető tantárgyak listája a bejelentkezett felhasználónak.
        /// Csak azok jelennek meg, amelyeket még nem vett fel, és van szabad hely.
        [HttpGet("available")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAvailableSubjects()
        {
            int studentId = GetCurrentUserId();
            var subjects = await _subjectService.GetAvailableSubjectsAsync(studentId);
            return Ok(subjects);
        }


        /// GET /api/subject/my-enrollments
        /// A bejelentkezett felhasználó által felvett tantárgyak.
        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            int studentId = GetCurrentUserId();
            var enrollments = await _subjectService.GetMyEnrollmentsAsync(studentId);
            return Ok(enrollments);
        }


        /// POST /api/subject/enroll
        /// Tantárgy felvétele. Body: { "subjectID": 3 }
        [HttpPost("enroll")]
        [Authorize(Roles = "Student")]
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


        /// DELETE /api/subject/drop/{subjectId}
        /// Tantárgy leadása.
        [HttpDelete("drop/{subjectId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Drop(int subjectId)
        {
            int studentId = GetCurrentUserId();
            var response = await _subjectService.DropAsync(studentId, subjectId);

            if (response.Status == "ERROR")
                return BadRequest(response);

            return Ok(response);
        }


        /// POST /api/subject/enroll-custom
        /// Egyedi (nem listában szereplő) tantárgy felvétele név alapján.
        /// Body: { "subjectName": "Kvantumfizika", "subjectCode": "KF001" }
        [HttpPost("enroll-custom")]
        [Authorize(Roles = "Student")]
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

        /// GET /api/subject/all
        /// Összes tantárgy listája (tanároknak és adminnak).
        [HttpGet("all")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return Ok(subjects);
        }
    }
}
