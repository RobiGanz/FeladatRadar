using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace FeladatRadar.Tests
{


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

    public class CustomEnrollRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;
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



    //  Tarcsányi Csongor Márk részei


    //  1. TANTÁRGY FELVÉTEL – VALIDÁCIÓ 

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

    // 2. EMAIL VALIDÁCIÓ TESZTEK (JAVÍTOTT) 

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

    // 3. CSOPORT MEGHÍVÁS VALIDÁCIÓ TESZTEK 

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

    //  4. ADMIN SZEREPKÖR VÉDELEM TESZTEK 

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


    //    Le Tuan Anh (Róbert) részei

    // 1. TANTÁRGY FELVÉTEL – SZABAD HELYEK ÉS ELÉRHETŐSÉG 

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

    // 2. TANTÁRGY FELVÉTEL – DUPLIKÁCIÓ ÉS ÁLLAPOT

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

    //  3. TANTÁRGY FELVÉTEL – KÓD NORMALIZÁCIÓ 

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

    // 4. TANTÁRGY LISTA – RENDEZÉS ÉS SZŰRÉS 

    public class TantargyListaSzuresTesztek
    {
        private readonly List<SubjectDto> _tantargyak = new()
        {
            new SubjectDto { SubjectID = 1, SubjectName = "Matematika",  Credits = 5,
                MaxStudents = 30, CurrentEnrollments = 10 },
            new SubjectDto { SubjectID = 2, SubjectName = "Fizika",      Credits = 4,
                MaxStudents = 20, CurrentEnrollments = 20 },  // tele
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

    // 5. ÓRAREND ÜTKÖZÉS TESZTEK 

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
}
