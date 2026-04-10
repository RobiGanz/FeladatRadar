using System.ComponentModel.DataAnnotations;

namespace FeladatRadar.backend.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "A felhasználónév kötelező!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "A felhasználónév 3-50 karakter között kell legyen.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó kötelező!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A jelszó legalább 6 karakter kell legyen.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Az e-mail kötelező!")]
        [EmailAddress(ErrorMessage = "Érvénytelen e-mail formátum.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A keresztnév kötelező!")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A vezetéknév kötelező!")]
        public string LastName { get; set; } = string.Empty;

        // Admin szerepkör publikusan NEM regisztrálható – csak Student vagy Teacher
        [Required]
        [RegularExpression("^(Student|Teacher)$", ErrorMessage = "Érvénytelen szerepkör.")]
        public string UserRole { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "A felhasználónév kötelező!")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó kötelező!")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }

    public class UpdateUsernameRequest
    {
        [Required(ErrorMessage = "Az új felhasználónév kötelező.")]
        [StringLength(50, MinimumLength = 3)]
        public string NewUsername { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "A jelenlegi jelszó kötelező.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Az új jelszó kötelező.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Az új jelszó legalább 6 karakter kell legyen.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
