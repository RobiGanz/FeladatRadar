using System.ComponentModel.DataAnnotations;

namespace FeladatRadar.backend.Models
{

    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentEnrollments { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive
        public int? TeacherID { get; set; }
        public string? TeacherName { get; set; }
    }

    // Hallgató által felvett tantárgy
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string EnrollmentStatus { get; set; } = "Active"; // Aktív, Eldobott, Befejezett
    }

    // Tantárgy felvételéhez szükséges kérés
    public class EnrollRequest
    {
        [Required]
        public int SubjectID { get; set; }
    }

    // Egyedi (nem listában szereplő) tantárgy felvételéhez
    public class CustomEnrollRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;
    }


    public class SubjectResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
