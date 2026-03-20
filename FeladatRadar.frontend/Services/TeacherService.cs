using FeladatRadar.frontend.Models;

namespace FeladatRadar.frontend.Services
{
    public class TeacherService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public TeacherService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        private async Task<SubjectResponse> ParseResponse(HttpResponseMessage response)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<SubjectResponse>();
                return result ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
            }
            catch
            {
                return new SubjectResponse
                {
                    Status = "ERROR",
                    Message = $"Szerver hiba: {response.StatusCode}"
                };
            }
        }

        // ──────────────────────────────
        // SZAVAZÁS
        // ──────────────────────────────

        public async Task<SubjectResponse> CreatePollAsync(int groupId, string question,
            List<string> options, DateTime? expiresAt = null)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Teacher/poll/create", new
                {
                    groupID = groupId,
                    question,
                    options,
                    expiresAt
                });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<List<PollDto>> GetGroupPollsAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Teacher/{groupId}/polls");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<PollDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<SubjectResponse> VoteAsync(int pollId, int optionId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"api/Teacher/poll/{pollId}/vote", new { optionID = optionId });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeletePollAsync(int pollId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Teacher/poll/{pollId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        // ──────────────────────────────
        // TANÁRI DOLGOZAT
        // ──────────────────────────────

        public async Task<SubjectResponse> CreateExamAsync(int groupId, string title,
            DateTime examDate, string? description = null, string? subjectName = null)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Teacher/exam/create", new
                {
                    groupID = groupId,
                    title,
                    description,
                    examDate,
                    subjectName
                });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<List<TeacherExamDto>> GetGroupExamsAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Teacher/{groupId}/exams");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<TeacherExamDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<SubjectResponse> DeleteExamAsync(int examId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Teacher/exam/{examId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }
    }

}
