using System.ComponentModel.DataAnnotations;

namespace FeladatRadar.frontend.Models
{
    public class RegisterRequest : IValidatableObject
    {
        [Required(ErrorMessage = "A felhasználónév kötelező!")]
        [MinLength(3, ErrorMessage = "A felhasználónév legalább 3 karakter legyen!")]
        [MaxLength(50, ErrorMessage = "A felhasználónév maximum 50 karakter lehet!")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Az email cím kötelező!")]
        [EmailAddress(ErrorMessage = "Érvénytelen email formátum! (pl: neve@email.com)")]
        [MaxLength(100, ErrorMessage = "Az email maximum 100 karakter lehet!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó kötelező!")]
        [MinLength(8, ErrorMessage = "A jelszó legalább 8 karakter legyen!")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "A jelszónak tartalmaznia kell legalább 1 nagybetűt és 1 számot!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megerősítése kötelező!")]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A keresztnév kötelező!")]
        [MaxLength(50, ErrorMessage = "A keresztnév maximum 50 karakter lehet!")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A vezetéknév kötelező!")]
        [MaxLength(50, ErrorMessage = "A vezetéknév maximum 50 karakter lehet!")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A szerepkör kiválasztása kötelező!")]
        public string UserRole { get; set; } = "Student";

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(
                    "A jelszavak nem egyeznek!",
                    new[] { nameof(ConfirmPassword) }
                );
            }
        }
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
}
