using FeladatRadar.frontend.Models;

namespace FeladatRadar.frontend.Services;
public class SubjectService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public SubjectService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<List<SubjectDto>> GetAvailableSubjectsAsync()
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.GetAsync("api/Subject/available");
            if (!response.IsSuccessStatusCode) return new List<SubjectDto>();
            var result = await response.Content.ReadFromJsonAsync<List<SubjectDto>>();
            return result ?? new List<SubjectDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SubjectService] GetAvailableSubjects error: {ex.Message}");
            return new List<SubjectDto>();
        }
    }

    public async Task<List<EnrollmentDto>> GetMyEnrollmentsAsync()
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.GetAsync("api/Subject/my-enrollments");
            if (!response.IsSuccessStatusCode) return new List<EnrollmentDto>();
            var result = await response.Content.ReadFromJsonAsync<List<EnrollmentDto>>();
            return result ?? new List<EnrollmentDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SubjectService] GetMyEnrollments error: {ex.Message}");
            return new List<EnrollmentDto>();
        }
    }

    public async Task<SubjectResponse> EnrollAsync(int subjectId)
    {
        await _authService.InitializeAsync();
        var response = await _httpClient.PostAsJsonAsync("api/Subject/enroll", new { subjectID = subjectId });
        return await response.Content.ReadFromJsonAsync<SubjectResponse>()
               ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
    }

    public async Task<SubjectResponse> DropAsync(int subjectId)
    {
        await _authService.InitializeAsync();
        var response = await _httpClient.DeleteAsync($"api/Subject/drop/{subjectId}");
        return await response.Content.ReadFromJsonAsync<SubjectResponse>()
               ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
    }

    public async Task<SubjectResponse> EnrollCustomAsync(string subjectName, string subjectCode)
    {
        await _authService.InitializeAsync();
        var response = await _httpClient.PostAsJsonAsync("api/Subject/enroll-custom", new
        {
            subjectName,
            subjectCode
        });
        return await response.Content.ReadFromJsonAsync<SubjectResponse>()
               ?? new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba." };
    }
}
