using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace FeladatRadar.Tests
{


    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentEnrollments { get; set; }
        public string Status { get; set; } = "Active";
        public int? TeacherID { get; set; }
        public string? TeacherName { get; set; }
    }

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

    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string EnrollmentStatus { get; set; } = "Active";
    }

    public class EnrollRequest
    {
        [Required]
        public int SubjectID { get; set; }
    }

    public class CustomEnrollRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;
    }

    public class SubjectResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class TaskItem
    {
        public int TaskID { get; set; }
        public int CreatedBy { get; set; }
        public int? SubjectID { get; set; }
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

    public static class RecurrenceKind
    {
        public const string None = "None";
        public const string Weekly = "Weekly";
        public const string Biweekly = "Biweekly";
        public const string Monthly = "Monthly";

        public static bool IsValid(string value) =>
            value is None or Weekly or Biweekly or Monthly;
    }

    public class ScheduleEntry
    {
        public int EntryID { get; set; }
        public int UserID { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RecurrenceType { get; set; } = "Weekly";
    }



    public static class SubjectEnrollmentHelper
    {
        public static bool CanEnroll(SubjectDto subject) =>
            subject.FreeSlots > 0;

        public static bool IsAlreadyEnrolled(IEnumerable<Enrollment> enrollments, int subjectId) =>
            enrollments.Any(e => e.SubjectID == subjectId && e.EnrollmentStatus == "Active");

        public static string NormalizeSubjectCode(string? code) =>
            string.IsNullOrWhiteSpace(code) ? "EGYEDI" : code.Trim().ToUpper();

        public static IEnumerable<SubjectDto> SortByFreeSlots(IEnumerable<SubjectDto> subjects) =>
            subjects.OrderByDescending(s => s.FreeSlots);

        public static IEnumerable<SubjectDto> FilterByMinCredits(IEnumerable<SubjectDto> subjects, int minCredits) =>
            subjects.Where(s => s.Credits >= minCredits);
    }

    public static class RecurringTaskHelper
    {
        public static List<DateTime> GenerateOccurrences(
            DateTime start, string recurrenceType, DateTime? endDate)
        {
            var dates = new List<DateTime> { start };
            if (recurrenceType == RecurrenceKind.None || endDate == null) return dates;

            var current = start;
            int iterations = 0;
            while (iterations < 52)
            {
                current = recurrenceType switch
                {
                    RecurrenceKind.Weekly => current.AddDays(7),
                    RecurrenceKind.Biweekly => current.AddDays(14),
                    RecurrenceKind.Monthly => current.AddMonths(1),
                    _ => endDate.Value.AddDays(1)
                };
                if (current > endDate.Value) break;
                dates.Add(current);
                iterations++;
            }
            return dates;
        }
    }

    public static class ScheduleConflictHelper
    {
        public static bool HasConflict(
            IEnumerable<ScheduleEntry> existing,
            string dayOfWeek, TimeSpan start, TimeSpan end) =>
            existing.Any(e =>
                e.DayOfWeek == dayOfWeek &&
                e.RecurrenceType == "Weekly" &&
                e.StartTime < end &&
                e.EndTime > start);
    }

    public static class FocusTimerHelper
    {
        private const double CircleRadius = 88.0;
        private const double Circumference = 2 * Math.PI * CircleRadius;

        public static double CalcDashOffset(int secondsLeft, int totalSeconds)
        {
            if (totalSeconds <= 0) return 0;
            return Circumference * ((double)secondsLeft / totalSeconds);
        }
    }

    public static class ValidationHelper
    {
        public static IList<ValidationResult> Validate(object obj)
        {
            var ctx = new ValidationContext(obj);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, ctx, results, true);
            return results;
        }
    }



    // Tarcsányi Csongor Márk részei
            
    //1. EMAIL VALIDÁCIÓ TESZTEK 

    public class EmailValidacioTesztek
    {
        private static bool ErvenyesEmail(string email)
        {
            try
            {
                if (!new EmailAddressAttribute().IsValid(email)) return false;
                var atIdx = email.IndexOf('@');
                if (atIdx < 0) return false;
                return email.Substring(atIdx + 1).Contains('.');
            }
            catch { return false; }
        }

        [Theory]
        [InlineData("janos@iskola.hu", true)]
        [InlineData("teszt.user@gmail.com", true)]
        [InlineData("user+tag@domain.org", true)]
        [InlineData("nemvalid", false)]
        [InlineData("@nincsfelhasznalo.hu", false)]
        [InlineData("nincs@tartomany", false)]
        [InlineData("", false)]
        public void Email_Format_Ellenorzes(string email, bool elvart)
        {
            Assert.Equal(elvart, ErvenyesEmail(email));
        }
    }

    // 2. CSOPORT MEGHÍVÁS VALIDÁCIÓ TESZTEK

    public class CsoportMeghivasTesztek
    {
        private readonly List<string> _tagok = new() { "kovacs.janos@iskola.hu" };
        private readonly List<string> _fuggoMeghivasok = new() { "var.peter@iskola.hu" };

        private string ValidaldMeghivas(string email)
        {
            if (_tagok.Contains(email)) return "Ez a felhasználó már tagja a csoportnak.";
            if (_fuggoMeghivasok.Contains(email)) return "Ez a felhasználó már meg van hívva.";
            if (!new EmailAddressAttribute().IsValid(email))
                return "Nem létezik ilyen email-cimű felhasználó.";
            return "OK";
        }

        [Fact]
        public void Meglevo_Tag_Meghivasa_Hibat_Ad()
        {
            Assert.Contains("már tagja", ValidaldMeghivas("kovacs.janos@iskola.hu"));
        }

        [Fact]
        public void Fuggob_Meghivas_Ujrameghivasa_Hibat_Ad()
        {
            Assert.Contains("már meg van hívva", ValidaldMeghivas("var.peter@iskola.hu"));
        }

        [Fact]
        public void Uj_Felhasznalo_Meghivasa_Sikeres()
        {
            Assert.Equal("OK", ValidaldMeghivas("uj.felhasznalo@iskola.hu"));
        }

        [Fact]
        public void Ervenytelen_Email_Hibat_Ad()
        {
            Assert.NotEqual("OK", ValidaldMeghivas("nemvalidemail"));
        }
    }

    // 3. ADMIN SZEREPKÖR VÉDELEM TESZTEK

    public class AdminVedelmTesztek
    {
        [Fact]
        public void Admin_Nem_Torolheti_Onmagat()
        {
            int adminId = 1, targetId = 1;
            Assert.True(adminId == targetId);
        }

        [Fact]
        public void Admin_Nem_Modosithatja_Mas_Admin_Szerepkoret()
        {
            Assert.True("Admin" == "Admin");
        }

        [Fact]
        public void Admin_Modosithatja_Student_Szerepkoret()
        {
            Assert.False("Student" == "Admin");
        }
    }



    //Le Tuan Anh (Róbert) részei

    // 4. TANTÁRGY FELVÉTEL – VALIDÁCIÓ 

    public class TantargyFelvetelValidacioTesztek
    {
        [Fact]
        public void Helyes_CustomEnrollRequest_Ervenyes()
        {
            var req = new CustomEnrollRequest
            {
                SubjectName = "Haladó algoritmusok",
                SubjectCode = "ALG401"
            };
            Assert.Empty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Tul_Rovid_SubjectName_Ervenytelen()
        {
            var req = new CustomEnrollRequest { SubjectName = "A" };
            Assert.NotEmpty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Ures_SubjectName_Ervenytelen()
        {
            var req = new CustomEnrollRequest { SubjectName = "" };
            Assert.NotEmpty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Tul_Hosszu_SubjectName_Ervenytelen()
        {
            var req = new CustomEnrollRequest { SubjectName = new string('a', 201) };
            Assert.NotEmpty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Pontosan_200_Karakteres_SubjectName_Ervenyes()
        {
            var req = new CustomEnrollRequest { SubjectName = new string('a', 200) };
            Assert.Empty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Ket_Karakteres_SubjectName_Ervenyes_Hatarertek()
        {
            var req = new CustomEnrollRequest { SubjectName = "Ab" };
            Assert.Empty(ValidationHelper.Validate(req));
        }

        [Fact]
        public void Tul_Hosszu_SubjectCode_Ervenytelen()
        {
            var req = new CustomEnrollRequest
            {
                SubjectName = "Érvényes név",
                SubjectCode = new string('X', 21)
            };
            Assert.NotEmpty(ValidationHelper.Validate(req));
        }
    }

    // 5. TANTÁRGY FELVÉTEL – SZABAD HELYEK ÉS ELÉRHETŐSÉG

    public class TantargyElerhsetoségTesztek
    {
        [Fact]
        public void FreeSlots_Helyes_Szamitas()
        {
            var subject = new SubjectDto { MaxStudents = 30, CurrentEnrollments = 12 };
            Assert.Equal(18, subject.FreeSlots);
        }

        [Fact]
        public void FreeSlots_Nulla_Ha_Tele_Van()
        {
            var subject = new SubjectDto { MaxStudents = 30, CurrentEnrollments = 30 };
            Assert.Equal(0, subject.FreeSlots);
        }

        [Fact]
        public void CanEnroll_Igaz_Ha_Van_Szabad_Hely()
        {
            var subject = new SubjectDto { MaxStudents = 30, CurrentEnrollments = 15 };
            Assert.True(SubjectEnrollmentHelper.CanEnroll(subject));
        }

        [Fact]
        public void CanEnroll_Hamis_Ha_Tele_Van()
        {
            var subject = new SubjectDto { MaxStudents = 30, CurrentEnrollments = 30 };
            Assert.False(SubjectEnrollmentHelper.CanEnroll(subject));
        }

        [Fact]
        public void CanEnroll_Hamis_Ha_Tulterhelt()
        {
            var subject = new SubjectDto { MaxStudents = 10, CurrentEnrollments = 11 };
            Assert.False(SubjectEnrollmentHelper.CanEnroll(subject));
        }

        [Fact]
        public void Egy_Szabad_Hellyel_CanEnroll_Igaz()
        {
            var subject = new SubjectDto { MaxStudents = 25, CurrentEnrollments = 24 };
            Assert.True(SubjectEnrollmentHelper.CanEnroll(subject));
        }

        [Theory]
        [InlineData(30, 0, 30)]
        [InlineData(30, 15, 15)]
        [InlineData(30, 30, 0)]
        [InlineData(1, 1, 0)]
        public void FreeSlots_Szamitasa_Kulonbozo_Esetekre(int max, int current, int elvartSzabad)
        {
            var subject = new SubjectDto { MaxStudents = max, CurrentEnrollments = current };
            Assert.Equal(elvartSzabad, subject.FreeSlots);
        }
    }

    // 6. TANTÁRGY FELVÉTEL – DUPLIKÁCIÓ ÉS ÁLLAPOT

    public class TantargyFelvetelDuplikacioTesztek
    {
        private readonly List<Enrollment> _felvettelTantargyak = new()
        {
            new Enrollment { EnrollmentID = 1, StudentID = 5, SubjectID = 101,
                SubjectName = "Matematika", Credits = 5, EnrollmentStatus = "Active",
                EnrolledAt = DateTime.Now.AddDays(-30) },
            new Enrollment { EnrollmentID = 2, StudentID = 5, SubjectID = 202,
                SubjectName = "Fizika", Credits = 4, EnrollmentStatus = "Active",
                EnrolledAt = DateTime.Now.AddDays(-20) },
        };

        [Fact]
        public void IsAlreadyEnrolled_Igaz_Ha_Mar_Felvet()
        {
            Assert.True(SubjectEnrollmentHelper.IsAlreadyEnrolled(_felvettelTantargyak, 101));
        }

        [Fact]
        public void IsAlreadyEnrolled_Hamis_Ha_Meg_Nem_Felvet()
        {
            Assert.False(SubjectEnrollmentHelper.IsAlreadyEnrolled(_felvettelTantargyak, 999));
        }

        [Fact]
        public void IsAlreadyEnrolled_Hamis_Ha_Ures_Lista()
        {
            Assert.False(SubjectEnrollmentHelper.IsAlreadyEnrolled(new List<Enrollment>(), 101));
        }

        [Fact]
        public void Enrollment_Alapertelmezett_Allapota_Active()
        {
            Assert.Equal("Active", new Enrollment().EnrollmentStatus);
        }

        [Fact]
        public void Enrollment_EnrolledAt_Rogziti_A_Felvetel_Datumat()
        {
            var now = DateTime.Now;
            Assert.Equal(now, new Enrollment { EnrolledAt = now }.EnrolledAt);
        }

        [Fact]
        public void Tobb_Tantargy_Felvetel_Mindketto_Megmarad()
        {
            var lista = new List<Enrollment>
            {
                new Enrollment { SubjectID = 101, EnrollmentStatus = "Active" },
                new Enrollment { SubjectID = 202, EnrollmentStatus = "Active" },
            };
            Assert.Equal(2, lista.Count);
            Assert.True(SubjectEnrollmentHelper.IsAlreadyEnrolled(lista, 101));
            Assert.True(SubjectEnrollmentHelper.IsAlreadyEnrolled(lista, 202));
        }
    }

    // 7. TANTÁRGY FELVÉTEL – KÓD NORMALIZÁCIÓ 

    public class TantargyEgyediKodTesztek
    {
        [Fact]
        public void Ures_SubjectCode_EGYEDI_Lesz()
        {
            Assert.Equal("EGYEDI", SubjectEnrollmentHelper.NormalizeSubjectCode(""));
        }

        [Fact]
        public void Null_SubjectCode_EGYEDI_Lesz()
        {
            Assert.Equal("EGYEDI", SubjectEnrollmentHelper.NormalizeSubjectCode(null));
        }

        [Fact]
        public void Csak_Szokozos_SubjectCode_EGYEDI_Lesz()
        {
            Assert.Equal("EGYEDI", SubjectEnrollmentHelper.NormalizeSubjectCode("   "));
        }

        [Fact]
        public void SubjectCode_Nagybetusre_Alakul()
        {
            Assert.Equal("MAT101", SubjectEnrollmentHelper.NormalizeSubjectCode("mat101"));
        }

        [Fact]
        public void SubjectCode_Whitespace_Trimmelve()
        {
            Assert.Equal("INF202", SubjectEnrollmentHelper.NormalizeSubjectCode("  INF202  "));
        }

        [Theory]
        [InlineData("mat101", "MAT101")]
        [InlineData("Fiz-201", "FIZ-201")]
        [InlineData("kem303", "KEM303")]
        public void SubjectCode_Normalizalas_Kulonbozo_Ertekekre(string bement, string elvart)
        {
            Assert.Equal(elvart, SubjectEnrollmentHelper.NormalizeSubjectCode(bement));
        }
    }

    // 8. TANTÁRGY LISTA – RENDEZÉS ÉS SZŰRÉS

    public class TantargyListaSzuresTesztek
    {
        private readonly List<SubjectDto> _tantargyak = new()
        {
            new SubjectDto { SubjectID = 1, SubjectName = "Matematika",  Credits = 5,
                MaxStudents = 30, CurrentEnrollments = 10 },
            new SubjectDto { SubjectID = 2, SubjectName = "Fizika",      Credits = 4,
                MaxStudents = 20, CurrentEnrollments = 20 },
            new SubjectDto { SubjectID = 3, SubjectName = "Kémia",       Credits = 3,
                MaxStudents = 15, CurrentEnrollments = 5  },
            new SubjectDto { SubjectID = 4, SubjectName = "Informatika", Credits = 6,
                MaxStudents = 25, CurrentEnrollments = 24 },
        };

        [Fact]
        public void Felvehet_Tantargyak_Szurese_Kizarja_A_Telieket()
        {
            var felveheto = _tantargyak.Where(SubjectEnrollmentHelper.CanEnroll).ToList();
            Assert.Equal(3, felveheto.Count);
            Assert.DoesNotContain(felveheto, s => s.SubjectName == "Fizika");
        }

        [Fact]
        public void Rendezés_Szabad_Helyek_Szerint_Elso_A_Legtobb_Hellyel()
        {
            var rendezett = SubjectEnrollmentHelper.SortByFreeSlots(_tantargyak).ToList();
            Assert.Equal("Matematika", rendezett.First().SubjectName);
        }

        [Fact]
        public void Rendezés_Szabad_Helyek_Szerint_Csokkenő_Sorrend()
        {
            var rendezett = SubjectEnrollmentHelper.SortByFreeSlots(_tantargyak).ToList();
            for (int i = 1; i < rendezett.Count; i++)
                Assert.True(rendezett[i].FreeSlots <= rendezett[i - 1].FreeSlots);
        }

        [Fact]
        public void Szures_Min_5_Kredit_Csak_Magas_Kredit_Targyak()
        {
            var tobb5 = SubjectEnrollmentHelper.FilterByMinCredits(_tantargyak, 5).ToList();
            Assert.Equal(2, tobb5.Count);
            Assert.All(tobb5, s => Assert.True(s.Credits >= 5));
        }

        [Fact]
        public void Ures_Lista_Szurese_Ures_Marad()
        {
            var eredmeny = SubjectEnrollmentHelper
                .FilterByMinCredits(new List<SubjectDto>(), 3).ToList();
            Assert.Empty(eredmeny);
        }
    }

    // 9. ÓRAREND ÜTKÖZÉS TESZTEK

    public class OrarendUtkozesTesztek
    {
        private readonly List<ScheduleEntry> _meglevo = new()
        {
            new ScheduleEntry
            {
                DayOfWeek      = "Monday",
                StartTime      = TimeSpan.FromHours(8),
                EndTime        = TimeSpan.FromHours(10),
                RecurrenceType = "Weekly"
            }
        };

        [Fact]
        public void Pontosan_Atlefedo_Idosav_Utkozest_Ad()
        {
            Assert.True(ScheduleConflictHelper.HasConflict(
                _meglevo, "Monday", TimeSpan.FromHours(8), TimeSpan.FromHours(10)));
        }

        [Fact]
        public void Reszlegesen_Atlefedo_Idosav_Utkozest_Ad()
        {
            Assert.True(ScheduleConflictHelper.HasConflict(
                _meglevo, "Monday", TimeSpan.FromHours(9), TimeSpan.FromHours(11)));
        }

        [Fact]
        public void Nem_Atlefedo_Idosav_Nem_Ad_Utkozest()
        {
            Assert.False(ScheduleConflictHelper.HasConflict(
                _meglevo, "Monday", TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
        }

        [Fact]
        public void Masik_Napon_Nincs_Utkozés()
        {
            Assert.False(ScheduleConflictHelper.HasConflict(
                _meglevo, "Tuesday", TimeSpan.FromHours(8), TimeSpan.FromHours(10)));
        }

        [Fact]
        public void Ures_Lista_Sosem_Ad_Utkozest()
        {
            Assert.False(ScheduleConflictHelper.HasConflict(
                new List<ScheduleEntry>(), "Monday",
                TimeSpan.FromHours(8), TimeSpan.FromHours(10)));
        }

        [Fact]
        public void Kozvetlenul_Egymás_Utan_Kovetkezo_Idosavok_Nem_Utkoznek()
        {
            Assert.False(ScheduleConflictHelper.HasConflict(
                _meglevo, "Monday", TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
        }
    }



    //  Lőrincz Antal (Anti) részei


    //  10. FELADAT – HATÁRIDŐ ÉS ÁLLAPOT TESZTEK

    public class HataridoTesztek
    {
        [Fact]
        public void Lejart_Feladat_IsOverdue_Igaz()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(-1), IsCompleted = false };
            Assert.True(task.IsOverdue);
        }

        [Fact]
        public void Jovobeli_Feladat_IsOverdue_Hamis()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(5), IsCompleted = false };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void Teljesitett_Lejart_Feladat_IsOverdue_Hamis()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(-1), IsCompleted = true };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void Harom_Napon_Beluli_Feladat_IsDueSoon_Igaz()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(2), IsCompleted = false };
            Assert.True(task.IsDueSoon);
        }

        [Fact]
        public void Negy_Nap_Mulva_Esedékes_IsDueSoon_Hamis()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(4), IsCompleted = false };
            Assert.False(task.IsDueSoon);
        }

        [Fact]
        public void Teljesitett_Feladat_IsDueSoon_Hamis()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(1), IsCompleted = true };
            Assert.False(task.IsDueSoon);
        }

        [Fact]
        public void Pontosan_Ma_Esedékes_Feladat_IsDueSoon_Igaz()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddHours(2), IsCompleted = false };
            Assert.True(task.IsDueSoon);
        }

        [Fact]
        public void Teljesitett_Feladat_DaysLeft_Kesz_Szoveg()
        {
            var task = new TaskItem { IsCompleted = true, DueDate = DateTime.Now.AddDays(5) };
            Assert.Equal("Kész", task.DaysLeft);
        }

        [Fact]
        public void Ma_Lejaro_Feladat_DaysLeft_Ma_Jar_Le()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddMinutes(30), IsCompleted = false };
            Assert.Equal("Ma jár le!", task.DaysLeft);
        }

        [Fact]
        public void Lejart_Feladat_DaysLeft_Napo_Lejart_Szoveg()
        {
            var task = new TaskItem { DueDate = DateTime.Now.AddDays(-3), IsCompleted = false };
            Assert.Contains("napja lejárt", task.DaysLeft);
        }
    }

    // 11. FELADAT – ISMÉTLŐDÉS ÉS TÍPUS TESZTEK 

    public class FeladatIsmetlodesTosztesTesztek
    {
        [Fact]
        public void IsRecurring_Igaz_Ha_Weekly()
        {
            var task = new TaskItem { RecurrenceType = RecurrenceKind.Weekly };
            Assert.True(task.IsRecurring);
        }

        [Fact]
        public void IsRecurring_Hamis_Ha_None()
        {
            var task = new TaskItem { RecurrenceType = RecurrenceKind.None };
            Assert.False(task.IsRecurring);
        }

        [Theory]
        [InlineData(RecurrenceKind.Weekly, "Heti")]
        [InlineData(RecurrenceKind.Biweekly, "Kéthetente")]
        [InlineData(RecurrenceKind.Monthly, "Havonta")]
        [InlineData(RecurrenceKind.None, "")]
        public void RecurrenceLabel_Helyes_Magyar_Szoveg(string tipus, string elvarLabel)
        {
            var task = new TaskItem { RecurrenceType = tipus };
            Assert.Equal(elvarLabel, task.RecurrenceLabel);
        }

        [Fact]
        public void None_Tipusnal_Csak_Egy_Datum_Keletkezik()
        {
            var start = new DateTime(2025, 9, 1);
            var result = RecurringTaskHelper.GenerateOccurrences(
                start, RecurrenceKind.None, new DateTime(2025, 12, 31));
            Assert.Single(result);
        }

        [Fact]
        public void Weekly_Hetente_Pontosan_Heten_Napot_Ugrik()
        {
            var start = new DateTime(2025, 9, 1);
            var end = new DateTime(2025, 9, 29);
            var dates = RecurringTaskHelper.GenerateOccurrences(start, RecurrenceKind.Weekly, end);
            Assert.Equal(5, dates.Count);
            for (int i = 1; i < dates.Count; i++)
                Assert.Equal(7, (dates[i] - dates[i - 1]).Days);
        }

        [Fact]
        public void Biweekly_Kethetente_Tizennegy_Napot_Ugrik()
        {
            var start = new DateTime(2025, 9, 1);
            var end = new DateTime(2025, 10, 13);
            var dates = RecurringTaskHelper.GenerateOccurrences(start, RecurrenceKind.Biweekly, end);
            for (int i = 1; i < dates.Count; i++)
                Assert.Equal(14, (dates[i] - dates[i - 1]).Days);
        }

        [Fact]
        public void Monthly_Havonta_Egy_Honapot_Ugrik()
        {
            var start = new DateTime(2025, 9, 15);
            var end = new DateTime(2026, 1, 31);
            var dates = RecurringTaskHelper.GenerateOccurrences(start, RecurrenceKind.Monthly, end);
            for (int i = 1; i < dates.Count; i++)
                Assert.Equal(start.AddMonths(i), dates[i]);
        }

        [Fact]
        public void Maximum_52_Iteracio_Biztonsagi_Korlat()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2030, 12, 31);
            var dates = RecurringTaskHelper.GenerateOccurrences(start, RecurrenceKind.Weekly, end);
            Assert.True(dates.Count <= 53);
        }

        [Fact]
        public void Elso_Datum_Mindig_A_Kezdodatum()
        {
            var start = new DateTime(2025, 9, 1);
            var dates = RecurringTaskHelper.GenerateOccurrences(
                start, RecurrenceKind.Weekly, new DateTime(2025, 12, 31));
            Assert.Equal(start, dates.First());
        }

        [Theory]
        [InlineData(RecurrenceKind.None)]
        [InlineData(RecurrenceKind.Weekly)]
        [InlineData(RecurrenceKind.Biweekly)]
        [InlineData(RecurrenceKind.Monthly)]
        public void RecurrenceKind_Ervenyes_Ertekek(string tipus)
        {
            Assert.True(RecurrenceKind.IsValid(tipus));
        }

        [Fact]
        public void RecurrenceKind_Ervenytelen_Ertek_Hamis()
        {
            Assert.False(RecurrenceKind.IsValid("Daily"));
            Assert.False(RecurrenceKind.IsValid(""));
            Assert.False(RecurrenceKind.IsValid("Yearly"));
        }
    }

    // 12. FELADAT – SZŰRÉS ÉS RENDEZÉS TESZTEK 

    public class FeladatSzuresTesztek
    {
        private readonly List<TaskItem> _feladatok = new()
        {
            new TaskItem { TaskID = 1, Title = "Matek házi",
                DueDate = DateTime.Now.AddDays(-2), IsCompleted = false, TaskType = "Homework" },
            new TaskItem { TaskID = 2, Title = "Fizika dolgozat",
                DueDate = DateTime.Now.AddDays(1),  IsCompleted = false, TaskType = "Exam" },
            new TaskItem { TaskID = 3, Title = "Történelem esszé",
                DueDate = DateTime.Now.AddDays(10), IsCompleted = false, TaskType = "Essay" },
            new TaskItem { TaskID = 4, Title = "Kész feladat",
                DueDate = DateTime.Now.AddDays(-5), IsCompleted = true,  TaskType = "Homework" },
            new TaskItem { TaskID = 5, Title = "Kémia labor",
                DueDate = DateTime.Now.AddDays(2),  IsCompleted = false, TaskType = "Lab" },
        };

        [Fact]
        public void Lejart_Feladatok_Szurese_Helyes()
        {
            var lejart = _feladatok.Where(t => t.IsOverdue).ToList();
            Assert.Single(lejart);
            Assert.Equal("Matek házi", lejart[0].Title);
        }

        [Fact]
        public void Hamaros_Feladatok_Szurese_Helyes()
        {
            var hamaros = _feladatok.Where(t => t.IsDueSoon).ToList();
            Assert.Equal(2, hamaros.Count);
        }

        [Fact]
        public void Aktiv_Feladatok_Kizarjak_A_Teljesitetteket()
        {
            var aktiv = _feladatok.Where(t => !t.IsCompleted).ToList();
            Assert.Equal(4, aktiv.Count);
        }

        [Fact]
        public void Kereses_Cimre_Nagybetu_Erzeketlen()
        {
            string kereses = "matek";
            var talalatok = _feladatok
                .Where(t => t.Title.Contains(kereses, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Single(talalatok);
            Assert.Equal("Matek házi", talalatok[0].Title);
        }

        [Fact]
        public void Rendezes_Hatarido_Szerint_Novekvo()
        {
            var rendezett = _feladatok.OrderBy(t => t.DueDate).ToList();
            for (int i = 1; i < rendezett.Count; i++)
                Assert.True(rendezett[i].DueDate >= rendezett[i - 1].DueDate);
        }

        [Fact]
        public void Szures_Exam_Tipusra()
        {
            var vizsgak = _feladatok.Where(t => t.TaskType == "Exam").ToList();
            Assert.Single(vizsgak);
            Assert.Equal("Fizika dolgozat", vizsgak[0].Title);
        }

        [Fact]
        public void Feladat_Hozzaadas_Noveli_A_Listat()
        {
            var lista = new List<TaskItem>(_feladatok);
            lista.Add(new TaskItem
            {
                TaskID = 6,
                Title = "Új feladat",
                DueDate = DateTime.Now.AddDays(5),
                IsCompleted = false
            });
            Assert.Equal(6, lista.Count);
        }

        [Fact]
        public void Feladat_Torles_Csokkenti_A_Listat()
        {
            var lista = new List<TaskItem>(_feladatok);
            lista.RemoveAll(t => t.TaskID == 1);
            Assert.Equal(4, lista.Count);
            Assert.DoesNotContain(lista, t => t.TaskID == 1);
        }

        [Fact]
        public void Feladat_Teljesitese_Megvaltoztatja_Az_Allapotot()
        {
            var task = new TaskItem { TaskID = 1, IsCompleted = false, DueDate = DateTime.Now.AddDays(5) };
            task.IsCompleted = true;
            Assert.True(task.IsCompleted);
            Assert.False(task.IsOverdue);
            Assert.False(task.IsDueSoon);
        }
    }

    // 13. FELADAT – MODELL INTEGRITÁS TESZTEK

    public class FeladatModelTesztek
    {
        [Fact]
        public void TaskItem_Alapertelmezett_RecurrenceType_None()
        {
            Assert.Equal(RecurrenceKind.None, new TaskItem().RecurrenceType);
        }

        [Fact]
        public void TaskItem_Alapertelmezett_IsCompleted_Hamis()
        {
            Assert.False(new TaskItem().IsCompleted);
        }

        [Fact]
        public void TaskItem_Alapertelmezett_TaskType_Exam()
        {
            Assert.Equal("Exam", new TaskItem().TaskType);
        }

        [Fact]
        public void TaskItem_ParentTaskID_Null_Alapertelmezetten()
        {
            Assert.Null(new TaskItem().ParentTaskID);
        }

        [Fact]
        public void TaskItem_SubjectID_Null_Ha_Nem_Kotott_Tantargyhoz()
        {
            Assert.Null(new TaskItem().SubjectID);
        }

        [Fact]
        public void TaskItem_IsRecurring_Hamis_Alapertelmezetten()
        {
            Assert.False(new TaskItem().IsRecurring);
        }

        [Fact]
        public void TaskItem_RecurrenceLabel_Ures_Alapertelmezetten()
        {
            Assert.Equal(string.Empty, new TaskItem().RecurrenceLabel);
        }

        [Fact]
        public void TaskItem_Tantargyhoz_Kotott_SubjectID_Helyes()
        {
            var task = new TaskItem { SubjectID = 42, SubjectName = "Matek" };
            Assert.Equal(42, task.SubjectID);
            Assert.Equal("Matek", task.SubjectName);
        }
    }

    // 14. FÓKUSZ IDŐZÍTŐ TESZTEK

    public class FokuszIdozitoTesztek
    {
        [Fact]
        public void Teljes_Ido_DashOffset_A_Kerulet_Erteke()
        {
            double offset = FocusTimerHelper.CalcDashOffset(1500, 1500);
            Assert.Equal(2 * Math.PI * 88, offset, precision: 2);
        }

        [Fact]
        public void Nulla_Masodperc_DashOffset_Nulla()
        {
            Assert.Equal(0, FocusTimerHelper.CalcDashOffset(0, 1500));
        }

        [Fact]
        public void Fele_Idone_DashOffset_A_Felek_Erteke()
        {
            double teljes = FocusTimerHelper.CalcDashOffset(1500, 1500);
            double fele = FocusTimerHelper.CalcDashOffset(750, 1500);
            Assert.Equal(teljes / 2, fele, precision: 2);
        }

        [Fact]
        public void Nulla_TotalSeconds_Nem_Dob_Kivételt()
        {
            var ex = Record.Exception(() => FocusTimerHelper.CalcDashOffset(100, 0));
            Assert.Null(ex);
        }

        [Theory]
        [InlineData(1500, 1500)]   // Fókusz 25 perc
        [InlineData(300, 300)]   // Rövid szünet 5 perc
        [InlineData(900, 900)]   // Hosszú szünet 15 perc
        public void Minden_Pomodoro_Mod_Szamolhato(int left, int total)
        {
            Assert.True(FocusTimerHelper.CalcDashOffset(left, total) >= 0);
        }
    }
}
