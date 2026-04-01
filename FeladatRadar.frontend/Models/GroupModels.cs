namespace FeladatRadar.frontend.Models
{
    public class GroupDto
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public bool IsOwner { get; set; }
        public string OwnerRole { get; set; } = "Student";
    }

    public class GroupMemberDto
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public bool IsOwner { get; set; }
    }

    public class GroupSubjectDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int OwnerID { get; set; }
    }

    public class GroupScheduleEntryDto
    {
        public int EntryID { get; set; }
        public int OwnerID { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string RecurrenceType { get; set; } = "Weekly";
        public DateTime? RecurrenceEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RecurrenceLabel => RecurrenceType switch
        {
            "Weekly"   => "Heti",
            "Biweekly" => "Kéthetente",
            "Monthly"  => "Havonta",
            _          => "Egyszeri"
        };
    }

    public class GroupTaskDto
    {
        public int TaskID { get; set; }
        public int OwnerID { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class AddGroupTaskRequest
    {
        public int SubjectID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = "Homework";
    }

    public class GroupInviteDto
    {
        public int InviteID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string InvitedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}