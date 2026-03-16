namespace FeladatRadar.frontend.Models
{
    public class TaskDto
    {
        public int TaskID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = "Exam";
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RecurrenceType { get; set; } = "None";
        public DateTime? RecurrenceEndDate { get; set; }
        public int? ParentTaskID { get; set; }

        public bool IsOverdue => !IsCompleted && DueDate < DateTime.Now;
        public bool IsDueSoon => !IsCompleted && !IsOverdue && DueDate <= DateTime.Now.AddDays(3);
        public bool IsRecurring => RecurrenceType != "None";

        public string RecurrenceLabel => RecurrenceType switch
        {
            "Weekly" => "Heti",
            "Biweekly" => "Kéthetente",
            "Monthly" => "Havonta",
            _ => ""
        };

        public string DaysLeft
        {
            get
            {
                if (IsCompleted) return "Kész";
                var diff = (DueDate - DateTime.Now).TotalDays;
                if (diff < 0) return $"{(int)Math.Abs(diff)} napja lejárt";
                if (diff < 1) return "Ma jár le!";
                return $"{(int)diff} nap múlva";
            }
        }
    }

    public class AddTaskRequest
    {
        public int? SubjectID { get; set; }  
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public string TaskType { get; set; } = "Exam";
        public string RecurrenceType { get; set; } = "None";
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
