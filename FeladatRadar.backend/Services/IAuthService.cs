using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
        Task<User?> GetUserById(int userId);
        string GenerateJwtToken(User user);
    }
}
