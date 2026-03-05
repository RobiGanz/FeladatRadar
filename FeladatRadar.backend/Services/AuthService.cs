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
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<LoginResponse> Register(RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"[API] Register attempt for: {request.Username}");

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
                    "sp_RegisterUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var result = results.FirstOrDefault();

                Console.WriteLine($"[API] Register SP result: {(result != null ? "Found" : "NULL")}");

                if (result == null)
                {
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = "Hiba történt a regisztráció során"
                    };
                }

                var resultDict = (IDictionary<string, object>)result;

                if (resultDict.ContainsKey("Status") && resultDict["Status"]?.ToString() == "ERROR")
                {
                    Console.WriteLine($"[API] Register error: {resultDict["Message"]}");
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = resultDict["Message"]?.ToString() ?? "Hiba"
                    };
                }

                var user = new User
                {
                    UserID = Convert.ToInt32(resultDict["UserID"]),
                    Username = resultDict["Username"]?.ToString() ?? "",
                    Email = resultDict["Email"]?.ToString(),
                    FirstName = resultDict["FirstName"]?.ToString(),
                    LastName = resultDict["LastName"]?.ToString(),
                    UserRole = resultDict["UserRole"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(resultDict["CreatedAt"])
                };

                Console.WriteLine($"[API] User registered: {user.Username}, ID: {user.UserID}");

                string token = GenerateJwtToken(user);

                Console.WriteLine($"[API] Register token generated: {token.Substring(0, Math.Min(20, token.Length))}...");

                return new LoginResponse
                {
                    Status = "SUCCESS",
                    Message = "Sikeres regisztráció!",
                    Token = token,
                    User = new UserDto
                    {
                        UserID = user.UserID,
                        Username = user.Username,
                        Email = user.Email ?? "",
                        FirstName = user.FirstName ?? "",
                        LastName = user.LastName ?? "",
                        UserRole = user.UserRole
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Register exception: {ex.Message}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");

                return new LoginResponse
                {
                    Status = "ERROR",
                    Message = $"Hiba: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"[API] ========== LOGIN START ==========");
                Console.WriteLine($"[API] Login attempt for: {request.Username}");
                Console.WriteLine($"[API] Connection string: {_connectionString.Substring(0, 30)}...");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine($"[API] Database connection opened successfully");

                var parameters = new DynamicParameters();
                parameters.Add("@Username", request.Username);

                var results = await connection.QueryAsync<dynamic>(
                    "sp_LoginUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var result = results.FirstOrDefault();

                Console.WriteLine($"[API] SP result: {(result != null ? "Found" : "NULL")}");

                if (result == null)
                {
                    Console.WriteLine($"[API] User not found in database");
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = "Hibás felhasználónév vagy jelszó!"
                    };
                }

                var resultDict = (IDictionary<string, object>)result;

                Console.WriteLine($"[API] Result contains {resultDict.Count} fields");

                if (resultDict.ContainsKey("Status") && resultDict["Status"]?.ToString() == "ERROR")
                {
                    Console.WriteLine($"[API] SP returned ERROR status");
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = "Hibás felhasználónév vagy jelszó!"
                    };
                }

                string storedPasswordHash = resultDict["PasswordHash"]?.ToString() ?? "";
                Console.WriteLine($"[API] Password hash retrieved: {storedPasswordHash.Substring(0, Math.Min(20, storedPasswordHash.Length))}...");

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, storedPasswordHash);
                Console.WriteLine($"[API] Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    Console.WriteLine($"[API] Password verification FAILED");
                    return new LoginResponse
                    {
                        Status = "ERROR",
                        Message = "Hibás felhasználónév vagy jelszó!"
                    };
                }

                Console.WriteLine($"[API] Creating User object...");

                var user = new User
                {
                    UserID = Convert.ToInt32(resultDict["UserID"]),
                    Username = resultDict["Username"]?.ToString() ?? "",
                    Email = resultDict["Email"]?.ToString(),
                    FirstName = resultDict["FirstName"]?.ToString(),
                    LastName = resultDict["LastName"]?.ToString(),
                    UserRole = resultDict["UserRole"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(resultDict["CreatedAt"])
                };

                Console.WriteLine($"[API] User object created:");
                Console.WriteLine($"[API]   - UserID: {user.UserID}");
                Console.WriteLine($"[API]   - Username: {user.Username}");
                Console.WriteLine($"[API]   - Email: {user.Email}");
                Console.WriteLine($"[API]   - FirstName: {user.FirstName}");
                Console.WriteLine($"[API]   - LastName: {user.LastName}");
                Console.WriteLine($"[API]   - UserRole: {user.UserRole}");

                Console.WriteLine($"[API] Generating JWT token...");
                string token = GenerateJwtToken(user);
                Console.WriteLine($"[API] Token generated: {token.Substring(0, Math.Min(30, token.Length))}...");

                var userDto = new UserDto
                {
                    UserID = user.UserID,
                    Username = user.Username,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    UserRole = user.UserRole
                };

                Console.WriteLine($"[API] UserDto created");

                var response = new LoginResponse
                {
                    Status = "SUCCESS",
                    Message = "Sikeres bejelentkezés!",
                    Token = token,
                    User = userDto
                };

                Console.WriteLine($"[API] LoginResponse created:");
                Console.WriteLine($"[API]   - Status: {response.Status}");
                Console.WriteLine($"[API]   - Message: {response.Message}");
                Console.WriteLine($"[API]   - Token: {(response.Token != null ? "EXISTS" : "NULL")}");
                Console.WriteLine($"[API]   - User: {(response.User != null ? "EXISTS" : "NULL")}");
                Console.WriteLine($"[API] ========== LOGIN SUCCESS ==========");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] ========== LOGIN EXCEPTION ==========");
                Console.WriteLine($"[API] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[API] Exception message: {ex.Message}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[API] =========================================");

                return new LoginResponse
                {
                    Status = "ERROR",
                    Message = $"Hiba: {ex.Message}"
                };
            }
        }

        public async Task<User?> GetUserById(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@UserID", userId);

            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_GetUserById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public string GenerateJwtToken(User user)
        {
            Console.WriteLine($"[API] GenerateJwtToken called for user: {user.Username}");

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured");

            Console.WriteLine($"[API] JWT SecretKey loaded (length: {secretKey.Length})");

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

            Console.WriteLine($"[API] Claims created: {claims.Length} claims");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"[API] Token string generated (length: {tokenString.Length})");

            return tokenString;
        }
    }
}
