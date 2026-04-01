namespace FeladatRadar.frontend.Models;
public class ScheduleEntryDto
{
    public int EntryID { get; set; }
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }          // 1=Hétfő ... 7=Vasárnap
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string RecurrenceType { get; set; } = "Weekly";
    public DateTime? RecurrenceEndDate { get; set; }
    /// <summary>Az első/pontos előfordulás dátuma.</summary>
    public DateTime? StartDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public string RecurrenceLabel => RecurrenceType switch
    {
        "Weekly"    => "Heti",
        "Biweekly"  => "Kéthetente",
        "Monthly"   => "Havonta",
        _           => "Egyszeri"
    };
}

public class AddScheduleRequest
{
    public int SubjectID { get; set; }
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string RecurrenceType { get; set; } = "Weekly";
    public DateTime? RecurrenceEndDate { get; set; }
    /// <summary>Az első/pontos előfordulás dátuma (frontend tölti ki).</summary>
    public DateTime? StartDate { get; set; }
}
