namespace FeladatRadar.backend.Models
{
    public class TaskItem
    {
        public int TaskID { get; set; }
        public int CreatedBy { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = "Exam";
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RecurrenceType { get; set; } = RecurrenceKind.None;   // alapból egyszeri
        public DateTime? RecurrenceEndDate { get; set; }
        public int? ParentTaskID { get; set; }   // ismétlődő sorozat első elemére mutat
    }

    public class AddTaskRequest
    {
        public int SubjectID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = "Exam";
        public string RecurrenceType { get; set; } = RecurrenceKind.None;
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
