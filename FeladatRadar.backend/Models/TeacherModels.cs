namespace FeladatRadar.backend.Models
{
    // ──────────────────────────────────────────
    // SZAVAZÁS
    // ──────────────────────────────────────────
    public class Poll
    {
        public int PollID { get; set; }
        public int GroupID { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public List<PollOption> Options { get; set; } = new();
        public int? MyVoteOptionID { get; set; }   // null = még nem szavazott
    }

    public class PollOption
    {
        public int OptionID { get; set; }
        public int PollID { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int VoteCount { get; set; }
    }

    public class CreatePollRequest
    {
        public int GroupID { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public DateTime? ExpiresAt { get; set; }
    }

    public class VoteRequest
    {
        public int OptionID { get; set; }
    }

    // ──────────────────────────────────────────
    // TANÁRI DOLGOZAT (naptárba, csak tanár kezeli)
    // ──────────────────────────────────────────
    public class TeacherExam
    {
        public int ExamID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ExamDate { get; set; }
        public string? SubjectName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTeacherExamRequest
    {
        public int GroupID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ExamDate { get; set; }
        public string? SubjectName { get; set; }
    }

    // ──────────────────────────────────────────
    // TANÁRI MEGHÍVÓ (Teacher meghív egy csoportba)
    // ──────────────────────────────────────────
    public class TeacherInviteRequest
    {
        public string InvitedEmail { get; set; } = string.Empty;
    }

}
