using Blazored.LocalStorage;
using FeladatRadar.frontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

        // Ennyi másodperc után "A szerver nem válaszol" üzenet jelenik meg
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

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
            using var cts = new CancellationTokenSource(RequestTimeout);
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request, cts.Token);

                if (!response.IsSuccessStatusCode)
                    return await ReadErrorResponse(response);

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cts.Token);
                if (result?.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    await ApplyLoginResult(result);

                return result;
            }
            catch (OperationCanceledException)
            {
                return new LoginResponse { Status = "ERROR", Message = "A szerver nem válaszol. Kérjük, próbáld újra később!" };
            }
            catch (HttpRequestException)
            {
                return new LoginResponse { Status = "ERROR", Message = "Nem sikerült kapcsolódni a szerverhez. Ellenőrizd az internetkapcsolatod!" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Váratlan hiba: {ex.Message}" };
            }
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            using var cts = new CancellationTokenSource(RequestTimeout);
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    return await ReadErrorResponse(response)
                        ?? new LoginResponse { Status = "ERROR", Message = "Hibás felhasználónév vagy jelszó!" };
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cts.Token);
                if (result?.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    await ApplyLoginResult(result);

                return result;
            }
            catch (OperationCanceledException)
            {
                return new LoginResponse { Status = "ERROR", Message = "A szerver nem válaszol. Kérjük, próbáld újra később!" };
            }
            catch (HttpRequestException)
            {
                return new LoginResponse { Status = "ERROR", Message = "Nem sikerült kapcsolódni a szerverhez. Ellenőrizd az internetkapcsolatod!" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Váratlan hiba: {ex.Message}" };
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

        /// <summary>
        /// Kiolvas a sikertelen HTTP válasz törzséből egy LoginResponse-t.
        /// Kezeli mind a LoginResponse, mind a ASP.NET ValidationProblemDetails formátumot.
        /// </summary>
        private static async Task<LoginResponse> ReadErrorResponse(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(body))
                    return new LoginResponse { Status = "ERROR", Message = $"Szerverhiba ({(int)response.StatusCode})" };

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

               
                if (root.TryGetProperty("message", out var msgProp) &&
                    !string.IsNullOrWhiteSpace(msgProp.GetString()))
                {
                    return new LoginResponse { Status = "ERROR", Message = msgProp.GetString()! };
                }

                
                if (root.TryGetProperty("errors", out var errors))
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                        foreach (var err in field.Value.EnumerateArray())
                            messages.Add(err.GetString() ?? "");
                    if (messages.Count > 0)
                        return new LoginResponse { Status = "ERROR", Message = string.Join(" | ", messages) };
                }

               
                if (root.TryGetProperty("title", out var title))
                    return new LoginResponse { Status = "ERROR", Message = title.GetString() ?? "Ismeretlen hiba" };

                return new LoginResponse { Status = "ERROR", Message = $"Szerverhiba ({(int)response.StatusCode})" };
            }
            catch
            {
                return new LoginResponse { Status = "ERROR", Message = $"Szerverhiba ({(int)response.StatusCode})" };
            }
        }
    }
}
