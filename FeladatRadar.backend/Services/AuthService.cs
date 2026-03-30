using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FeladatRadar.backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Kapcsolati sztring nem található.");
        }

        public async Task<LoginResponse> Register(RegisterRequest request)
        {
            try
            {
                // Admin szerepkört regisztrációkor nem lehet önállóan felvenni
                if (request.UserRole == "Admin")
                    return new LoginResponse { Status = "ERROR", Message = "Admin fiókot nem lehet regisztrációval létrehozni." };

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@Username", request.Username);
                parameters.Add("@PasswordHash", passwordHash);
                parameters.Add("@Email", request.Email);
                parameters.Add("@FirstName", request.FirstName);
                parameters.Add("@LastName", request.LastName);
                parameters.Add("@UserRole", request.UserRole);

                var results = await connection.QueryAsync<dynamic>(
                    "sp_RegisterUser", parameters, commandType: CommandType.StoredProcedure);

                var result = results.FirstOrDefault();
                if (result == null)
                    return new LoginResponse { Status = "ERROR", Message = "Hiba történt a regisztráció során." };

                var d = (IDictionary<string, object>)result;
                if (d["Status"]?.ToString() == "ERROR")
                    return new LoginResponse { Status = "ERROR", Message = d["Message"]?.ToString() ?? "Hiba" };

                var user = MapUser(d);
                return new LoginResponse
                {
                    Status = "SUCCESS",
                    Message = "Sikeres regisztráció!",
                    Token = GenerateJwtToken(user),
                    User = MapUserDto(user)
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Username", request.Username);

                var results = await connection.QueryAsync<dynamic>(
                    "sp_LoginUser", parameters, commandType: CommandType.StoredProcedure);

                var result = results.FirstOrDefault();
                if (result == null)
                    return new LoginResponse { Status = "ERROR", Message = "Hibás felhasználónév vagy jelszó!" };

                var d = (IDictionary<string, object>)result;
                if (d["Status"]?.ToString() == "ERROR")
                    return new LoginResponse { Status = "ERROR", Message = "Hibás felhasználónév vagy jelszó!" };

                string storedHash = d["PasswordHash"]?.ToString() ?? "";
                if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                    return new LoginResponse { Status = "ERROR", Message = "Hibás felhasználónév vagy jelszó!" };

                var user = MapUser(d);
                return new LoginResponse
                {
                    Status = "SUCCESS",
                    Message = "Sikeres bejelentkezés!",
                    Token = GenerateJwtToken(user),
                    User = MapUserDto(user)
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        public async Task<User?> GetUserById(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@UserID", userId);
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_GetUserById", parameters, commandType: CommandType.StoredProcedure);
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey nincs beállítva.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.UserRole),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<LoginResponse> UpdateUsername(int userId, string newUsername)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@UserID", userId);
                parameters.Add("@NewUsername", newUsername);

                var results = await connection.QueryAsync<dynamic>(
                    "sp_UpdateUsername", parameters, commandType: CommandType.StoredProcedure);

                var result = results.FirstOrDefault();
                if (result == null)
                    return new LoginResponse { Status = "ERROR", Message = "Hiba történt." };

                var d = (IDictionary<string, object>)result;
                if (d["Status"]?.ToString() == "ERROR")
                    return new LoginResponse { Status = "ERROR", Message = d["Message"]?.ToString() ?? "Hiba" };

                var user = MapUser(d);
                return new LoginResponse
                {
                    Status = "SUCCESS",
                    Message = "Felhasználónév sikeresen módosítva!",
                    Token = GenerateJwtToken(user),
                    User = MapUserDto(user)
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        public async Task<LoginResponse> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                // Jelenlegi hash lekérése az sp_LoginUser SP eredményéből való user alapján
                // (direkt lekérdezés, de paraméteres – SQL injection nem lehetséges)
                var hash = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT PasswordHash FROM Users WHERE UserID = @UserID AND IsActive = 1",
                    new { UserID = userId });

                if (string.IsNullOrEmpty(hash) || !BCrypt.Net.BCrypt.Verify(currentPassword, hash))
                    return new LoginResponse { Status = "ERROR", Message = "A jelenlegi jelszó helytelen!" };

                string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var parameters = new DynamicParameters();
                parameters.Add("@UserID", userId);
                parameters.Add("@NewPasswordHash", newHash);

                await connection.ExecuteAsync(
                    "sp_ChangePassword", parameters, commandType: CommandType.StoredProcedure);

                return new LoginResponse { Status = "SUCCESS", Message = "Jelszó sikeresen módosítva!" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        private static User MapUser(IDictionary<string, object> d) => new()
        {
            UserID = Convert.ToInt32(d["UserID"]),
            Username = d["Username"]?.ToString() ?? "",
            Email = d["Email"]?.ToString(),
            FirstName = d["FirstName"]?.ToString(),
            LastName = d["LastName"]?.ToString(),
            UserRole = d["UserRole"]?.ToString() ?? "",
            CreatedAt = Convert.ToDateTime(d["CreatedAt"])
        };

        private static UserDto MapUserDto(User user) => new()
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            UserRole = user.UserRole
        };
    }
}