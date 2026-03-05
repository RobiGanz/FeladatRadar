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

        // Public properties
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        public UserDto? CurrentUser => _currentUser;

        // Initialize from LocalStorage
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                _token = await _localStorage.GetItemAsync<string>("authToken");
                _currentUser = await _localStorage.GetItemAsync<UserDto>("currentUser");

                if (!string.IsNullOrEmpty(_token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);
                }

                _isInitialized = true;
                Console.WriteLine($"[AuthService] InitializeAsync - IsAuthenticated: {IsAuthenticated}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] InitializeAsync Error: {ex.Message}");
            }
        }

        public async Task<LoginResponse?> Register(RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"[AuthService] Register attempt for: {request.Username}");

                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (result != null && result.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    {
                        // Save to memory first
                        _token = result.Token;
                        _currentUser = result.User;
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", _token);

                        // Save to LocalStorage (can fail during prerendering)
                        try
                        {
                            await _localStorage.SetItemAsync("authToken", _token);
                            await _localStorage.SetItemAsync("currentUser", _currentUser);
                        }
                        catch
                        {
                            // Will be saved later in OnAfterRenderAsync
                            Console.WriteLine("[AuthService] LocalStorage save deferred");
                        }

                        Console.WriteLine($"[AuthService] Register SUCCESS - User: {_currentUser?.Username}");
                    }

                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = $"Hiba történt: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Status = "ERROR",
                    Message = $"Kapcsolódási hiba: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"[AuthService] Login attempt for: {request.Username}");

                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

                Console.WriteLine($"[AuthService] Login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    Console.WriteLine($"[AuthService] Login result status: {result?.Status}");
                    Console.WriteLine($"[AuthService] Token exists: {!string.IsNullOrEmpty(result?.Token)}");
                    Console.WriteLine($"[AuthService] User exists: {result?.User != null}");

                    if (result != null && result.Status == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
                    {
                        // Save to memory IMMEDIATELY
                        _token = result.Token;
                        _currentUser = result.User;
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", _token);

                        Console.WriteLine($"[AuthService] Login SUCCESS!");
                        Console.WriteLine($"[AuthService] Token: {_token.Substring(0, 20)}...");
                        Console.WriteLine($"[AuthService] User: {_currentUser?.Username}");
                        Console.WriteLine($"[AuthService] IsAuthenticated: {IsAuthenticated}");

                        // Try to save to LocalStorage (may fail during prerendering)
                        try
                        {
                            await _localStorage.SetItemAsync("authToken", _token);
                            await _localStorage.SetItemAsync("currentUser", _currentUser);
                            Console.WriteLine($"[AuthService] Saved to LocalStorage");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[AuthService] LocalStorage save failed (will retry): {ex.Message}");
                        }
                    }

                    return result;
                }
                else
                {
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = "Hibás felhasználónév vagy jelszó!"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] Login exception: {ex.Message}");

                return new LoginResponse
                {
                    Status = "ERROR",
                    Message = $"Kapcsolódási hiba: {ex.Message}"
                };
            }
        }

        // Method to save to LocalStorage after render
        public async Task SaveToLocalStorageAsync()
        {
            if (!string.IsNullOrEmpty(_token) && _currentUser != null)
            {
                try
                {
                    await _localStorage.SetItemAsync("authToken", _token);
                    await _localStorage.SetItemAsync("currentUser", _currentUser);
                    Console.WriteLine($"[AuthService] Saved to LocalStorage after render");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AuthService] Failed to save to LocalStorage: {ex.Message}");
                }
            }
        }

        public async Task<UserDto?> GetCurrentUser()
        {
            if (!IsAuthenticated) return null;

            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>("api/Auth/me");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] GetCurrentUser error: {ex.Message}");
                return null;
            }
        }

        public async Task Logout()
        {
            Console.WriteLine($"[AuthService] Logout called");

            _token = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;

            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("currentUser");

            Console.WriteLine($"[AuthService] Logout complete");
        }
    }
}
