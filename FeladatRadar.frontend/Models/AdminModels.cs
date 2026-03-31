namespace FeladatRadar.frontend.Models
{
    public class AdminUserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalGroups { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalTasks { get; set; }
        public int TotalPolls { get; set; }
        public int TotalExams { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int ActiveTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }
    }

    public class AdminGroupDto
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
    }

    public class AuditLogEntry
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminGroupExamDto
    {
        public int ExamID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ExamDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string? SubjectName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
