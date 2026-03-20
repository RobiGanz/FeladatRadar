using FeladatRadar.frontend.Models;

namespace FeladatRadar.frontend.Services
{
    public class AdminService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public AdminService(HttpClient httpClient, AuthService authService)
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
                return new SubjectResponse { Status = "ERROR", Message = $"Szerver hiba: {response.StatusCode}" };
            }
        }

        // ──────────────────────────────────────────
        // STATISZTIKÁK
        // ──────────────────────────────────────────

        public async Task<SystemStatsDto> GetSystemStatsAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/stats");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<SystemStatsDto>() ?? new();
            }
            catch { return new(); }
        }

        // ──────────────────────────────────────────
        // FELHASZNÁLÓK
        // ──────────────────────────────────────────

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/users");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<AdminUserDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<SubjectResponse> ChangeUserRoleAsync(int targetUserId, string newRole)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Admin/users/change-role", new
                {
                    targetUserID = targetUserId,
                    newRole
                });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> ToggleUserActiveAsync(int targetUserId, bool isActive)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Admin/users/toggle-active", new
                {
                    targetUserID = targetUserId,
                    isActive
                });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeleteUserAsync(int userId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/users/{userId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        // ──────────────────────────────────────────
        // CSOPORTOK
        // ──────────────────────────────────────────

        public async Task<List<AdminGroupDto>> GetAllGroupsAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/groups");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<AdminGroupDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<SubjectResponse> DeleteGroupAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/groups/{groupId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        // ──────────────────────────────────────────
        // MODERÁCIÓ
        // ──────────────────────────────────────────

        public async Task<SubjectResponse> DeletePollAsync(int pollId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/polls/{pollId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeleteExamAsync(int examId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/exams/{examId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeleteTaskAsync(int taskId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/tasks/{taskId}");
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        // ──────────────────────────────────────────
        // AUDIT LOG
        // ──────────────────────────────────────────

        public async Task<List<AuditLogEntry>> GetAuditLogAsync(int limit = 100)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Admin/audit-log?limit={limit}");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<AuditLogEntry>>() ?? new();
            }
            catch { return new(); }
        }
    }
}
