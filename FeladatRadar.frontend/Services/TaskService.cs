using FeladatRadar.frontend.Models;
using FeladatRadar.frontend.Services;
using System.Net.Http.Json;

namespace BlazorClient.Service;

public class TaskService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public TaskService(HttpClient httpClient, AuthService authService)
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

    public async Task<List<TaskDto>> GetMyTasksAsync()
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.GetAsync("api/Task/my-tasks");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<TaskDto>>() ?? new();
        }
        catch { return new(); }
    }

    public async Task<SubjectResponse> AddTaskAsync(AddTaskRequest request)
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Task/add", request);
            return await ParseResponse(response);
        }
        catch (Exception ex)
        {
            return new SubjectResponse { Status = "ERROR", Message = ex.Message };
        }
    }

    public async Task<SubjectResponse> CompleteTaskAsync(int taskId)
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.PutAsync($"api/Task/complete/{taskId}", null);
            return await ParseResponse(response);
        }
        catch (Exception ex)
        {
            return new SubjectResponse { Status = "ERROR", Message = ex.Message };
        }
    }

    public async Task<SubjectResponse> UncompleteTaskAsync(int taskId)
    {
        await _authService.InitializeAsync();
        try
        {
            var response = await _httpClient.PutAsync($"api/Task/uncomplete/{taskId}", null);
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
            var response = await _httpClient.DeleteAsync($"api/Task/delete/{taskId}");
            return await ParseResponse(response);
        }
        catch (Exception ex)
        {
            return new SubjectResponse { Status = "ERROR", Message = ex.Message };
        }
    }
}