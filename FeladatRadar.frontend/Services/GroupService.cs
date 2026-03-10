using FeladatRadar.frontend.Models;
using FeladatRadar.frontend.Services;
using System.Net.Http.Json;

namespace FeladatRadar.frontend.Service
{
    public class GroupService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public GroupService(HttpClient httpClient, AuthService authService)
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

        public async Task<List<GroupDto>> GetMyGroupsAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Group/my-groups");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<GroupInviteDto>> GetMyInvitesAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Group/my-invites");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupInviteDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<SubjectResponse> CreateGroupAsync(string groupName)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Group/create", new { groupName });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> InviteAsync(int groupId, string email)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Group/{groupId}/invite", new { invitedEmail = email });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> AcceptInviteAsync(int inviteId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Group/invite/{inviteId}/accept", new { });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeclineInviteAsync(int inviteId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Group/invite/{inviteId}/decline", new { });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> LeaveGroupAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Group/{groupId}/leave", new { });
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<List<GroupMemberDto>> GetMembersAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Group/{groupId}/members");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupMemberDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<GroupSubjectDto>> GetGroupSubjectsAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Group/{groupId}/subjects");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupSubjectDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<GroupScheduleEntryDto>> GetGroupScheduleAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Group/{groupId}/schedule");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupScheduleEntryDto>>() ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<GroupTaskDto>> GetGroupTasksAsync(int groupId)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync($"api/Group/{groupId}/tasks");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<GroupTaskDto>>() ?? new();
            }
            catch { return new(); }
        }
        public async Task<SubjectResponse> AddGroupScheduleEntryAsync(int groupId, AddScheduleRequest request)
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Group/{groupId}/schedule/add", request);
                return await ParseResponse(response);
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }
    }
}