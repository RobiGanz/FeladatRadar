using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
        Task<User?> GetUserById(int userId);
        string GenerateJwtToken(User user);
        Task<LoginResponse> UpdateUsername(int userId, string newUsername);
        Task<LoginResponse> ChangePassword(int userId, string currentPassword, string newPassword);
    }
}
