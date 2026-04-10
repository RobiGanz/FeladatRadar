using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeladatRadar.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeacherController : BaseController
    {
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;

        public TeacherController(ITeacherService teacherService, IGroupService groupService)
        {
            _teacherService = teacherService;
            _groupService = groupService;
        }

        private async Task<bool> CanManageGroup(int groupId)
            => await _groupService.CanManageGroupAsync(groupId, GetCurrentUserId());

        // ─── SZAVAZÁSOK ───────────────────────────────────────────────────────────

        /// <summary>Új szavazás létrehozása (csoporttulajdonos vagy tanár).</summary>
        [HttpPost("poll/create")]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "A kérdés megadása kötelező." });
            if (request.Options == null || request.Options.Count < 2)
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "Legalább 2 opció szükséges." });
            if (!await CanManageGroup(request.GroupID))
                return StatusCode(403, new SubjectResponse { Status = "ERROR", Message = "Nincs jogosultságod szavazást létrehozni." });
            var result = await _teacherService.CreatePollAsync(GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Csoport szavazásainak lekérdezése (minden tag látja).</summary>
        [HttpGet("{groupId}/polls")]
        public async Task<IActionResult> GetGroupPolls(int groupId)
        {
            var polls = await _teacherService.GetGroupPollsAsync(groupId, GetCurrentUserId());
            return Ok(polls);
        }

        /// <summary>Szavazat leadása (minden csoporttag).</summary>
        [HttpPost("poll/{pollId}/vote")]
        public async Task<IActionResult> Vote(int pollId, [FromBody] VoteRequest request)
        {
            var result = await _teacherService.VoteAsync(pollId, request.OptionID, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Szavazás törlése (csak a létrehozó).</summary>
        [HttpDelete("poll/{pollId}")]
        public async Task<IActionResult> DeletePoll(int pollId)
        {
            var result = await _teacherService.DeletePollAsync(pollId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        // ─── DOLGOZATOK ───────────────────────────────────────────────────────────

        /// <summary>Új dolgozat létrehozása (csak tanár szerepkörű csoporttag hozhat létre).</summary>
        [HttpPost("exam/create")]
        public async Task<IActionResult> CreateExam([FromBody] CreateTeacherExamRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new SubjectResponse { Status = "ERROR", Message = "A dolgozat neve kötelező." });

            var isMember = await _groupService.IsGroupMemberAsync(request.GroupID, GetCurrentUserId());
            if (!isMember || GetCurrentUserRole() != "Teacher")
                return StatusCode(403, new SubjectResponse { Status = "ERROR", Message = "Nincs jogosultságod dolgozatot létrehozni." });

            var result = await _teacherService.CreateExamAsync(GetCurrentUserId(), request);
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Csoport dolgozatainak lekérdezése.</summary>
        [HttpGet("{groupId}/exams")]
        public async Task<IActionResult> GetGroupExams(int groupId)
        {
            var exams = await _teacherService.GetGroupExamsAsync(groupId, GetCurrentUserId());
            return Ok(exams);
        }

        /// <summary>Dolgozat törlése (csak a létrehozó).</summary>
        [HttpDelete("exam/{examId}")]
        public async Task<IActionResult> DeleteExam(int examId)
        {
            var result = await _teacherService.DeleteExamAsync(examId, GetCurrentUserId());
            if (result.Status == "ERROR") return BadRequest(result);
            return Ok(result);
        }
    }
}
