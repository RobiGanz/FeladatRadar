namespace FeladatRadar.backend.Models
{
    // Ismétlődés típusok: None, Weekly, Biweekly, Monthly
    public static class RecurrenceKind
    {
        public const string None = "None";       // Egyszeri
        public const string Weekly = "Weekly";     // Heti
        public const string Biweekly = "Biweekly";  // Kéthetente
        public const string Monthly = "Monthly";   // Havonta
    }

    public class ScheduleEntry
    {
        public int EntryID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }          // 1=Hétfő ... 7=Vasárnap
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string RecurrenceType { get; set; } = RecurrenceKind.Weekly;
        public DateTime? RecurrenceEndDate { get; set; }
        /// <summary>
        /// Az első (pontos) előfordulás dátuma.
        /// Egyszeri bejegyzésnél: kizárólag ezen a napon jelenik meg.
        /// Kéthetente/Havonta: ettől a dátumtól számítja az ismétlődést.
        /// Heti: nem kötelező (visszafelé kompatibilitás).
        /// </summary>
        public DateTime? StartDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddScheduleRequest
    {
        public int SubjectID { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string RecurrenceType { get; set; } = RecurrenceKind.Weekly;
        public DateTime? RecurrenceEndDate { get; set; }
        /// <summary>Az első/pontos előfordulás dátuma (frontend küldi).</summary>
        public DateTime? StartDate { get; set; }
    }
}
