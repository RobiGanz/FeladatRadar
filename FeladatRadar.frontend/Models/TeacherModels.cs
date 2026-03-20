namespace FeladatRadar.frontend.Models
{
    public class PollDto
    {
        public int PollID { get; set; }
        public int GroupID { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public List<PollOptionDto> Options { get; set; } = new();
        public int? MyVoteOptionID { get; set; }

        public bool HasVoted => MyVoteOptionID.HasValue;
        public int TotalVotes => Options.Sum(o => o.VoteCount);
    }

    public class PollOptionDto
    {
        public int OptionID { get; set; }
        public int PollID { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int VoteCount { get; set; }
    }

    public class TeacherExamDto
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

}
