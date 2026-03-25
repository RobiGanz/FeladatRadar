using Blazored.LocalStorage;
using FeladatRadar.frontend.Models;
using System.Net.Http.Headers;

namespace FeladatRadar.frontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private string? _token;
        private UserDto? _currentUser;
        private bool _isInitialized = false;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        public UserDto? CurrentUser => _currentUser;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            try
            {
                _token = await _localStorage.GetItemAsync<string>("authToken");
                _currentUser = await _localStorage.GetItemAsync<UserDto>("currentUser");
                if (!string.IsNullOrEmpty(_token))
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);
                _isInitialized = true;
            }
            catch { }
        }

        public async Task<LoginResponse?> Register(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);
                if (!response.IsSuccessStatusCode)
                    return new LoginResponse { Status = "ERROR", Message = $"Hiba: {response.StatusCode}" };

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    await ApplyLoginResult(result);

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Kapcsolódási hiba: {ex.Message}" };
            }
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
                if (!response.IsSuccessStatusCode)
                    return new LoginResponse { Status = "ERROR", Message = "Hibás felhasználónév vagy jelszó!" };

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    await ApplyLoginResult(result);

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Kapcsolódási hiba: {ex.Message}" };
            }
        }

        public async Task SaveToLocalStorageAsync()
        {
            if (!string.IsNullOrEmpty(_token) && _currentUser != null)
            {
                try
                {
                    await _localStorage.SetItemAsync("authToken", _token);
                    await _localStorage.SetItemAsync("currentUser", _currentUser);
                }
                catch { }
            }
        }

        public async Task<UserDto?> GetCurrentUser()
        {
            if (!IsAuthenticated) return null;
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>("api/Auth/me");
            }
            catch { return null; }
        }

        public async Task Logout()
        {
            _token = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("currentUser");
        }

        public async Task<LoginResponse?> UpdateUsername(string newUsername)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    "api/Auth/update-username", new { NewUsername = newUsername });
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Status == "SUCCESS" && result.User != null)
                {
                    _currentUser = result.User;
                    if (!string.IsNullOrEmpty(result.Token))
                    {
                        _token = result.Token;
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", _token);
                        await _localStorage.SetItemAsync("authToken", _token);
                    }
                    await _localStorage.SetItemAsync("currentUser", _currentUser);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        public async Task<LoginResponse?> ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    "api/Auth/change-password",
                    new { CurrentPassword = currentPassword, NewPassword = newPassword });
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        private async Task ApplyLoginResult(LoginResponse result)
        {
            _token = result.Token;
            _currentUser = result.User;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token!);
            try
            {
                await _localStorage.SetItemAsync("authToken", _token);
                await _localStorage.SetItemAsync("currentUser", _currentUser);
            }
            catch { }
        }
    }
}