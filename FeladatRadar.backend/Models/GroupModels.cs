namespace FeladatRadar.backend.Models
{
    public class Group
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public bool IsOwner { get; set; }
        public string OwnerRole { get; set; } = "Student";
    }

    public class GroupMember
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public bool IsOwner { get; set; }
    }

    public class GroupSubject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int OwnerID { get; set; }
    }

    public class GroupScheduleEntry
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
    }

    public class GroupTask
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

    public class GroupInvite
    {
        public int InviteID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string InvitedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
    }

    public class InviteToGroupRequest
    {
        public string InvitedEmail { get; set; } = string.Empty;
    }

    public class AddGroupTaskRequest
    {
        public int SubjectID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = "Exam";
    }
}