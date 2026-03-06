using FeladatRadar.frontend.Models;

namespace FeladatRadar.frontend.Services;

    public class ScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ScheduleService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<ScheduleEntryDto>> GetMyScheduleAsync()
        {
            await _authService.InitializeAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/Schedule/my-schedule");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ScheduleService] GetMySchedule failed: {response.StatusCode}");
                    return new List<ScheduleEntryDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<List<ScheduleEntryDto>>();
                return result ?? new List<ScheduleEntryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScheduleService] GetMySchedule error: {ex.Message}");
                return new List<ScheduleEntryDto>();
            }
        }

        public async Task<SubjectResponse> AddEntryAsync(AddScheduleRequest request)
        {
            await _authService.InitializeAsync();

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Schedule/add", request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new SubjectResponse { Status = "ERROR", Message = "Nincs jogosultságod (401). Jelentkezz be újra!" };

                return await response.Content.ReadFromJsonAsync<SubjectResponse>()
                       ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        public async Task<SubjectResponse> DeleteEntryAsync(int entryId)
        {
            await _authService.InitializeAsync();
            var response = await _httpClient.DeleteAsync($"api/Schedule/delete/{entryId}");
            return await response.Content.ReadFromJsonAsync<SubjectResponse>()
                   ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
        }
    }
