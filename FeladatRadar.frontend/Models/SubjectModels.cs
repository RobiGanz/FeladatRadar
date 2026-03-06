namespace FeladatRadar.frontend.Models;
public class SubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int MaxStudents { get; set; }
    public int CurrentEnrollments { get; set; }
    public string? TeacherName { get; set; }
    public int FreeSlots => MaxStudents - CurrentEnrollments;
}

public class EnrollmentDto
{
    public int EnrollmentID { get; set; }
    public int SubjectID { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string EnrollmentStatus { get; set; } = string.Empty;
}

public class SubjectResponse
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}