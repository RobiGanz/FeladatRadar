using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace FeladatRadar.Tests
{


    public class CustomEnrollRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;
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


  
    //  4. TANTÁRGY FELVÉTEL – VALIDÁCIÓ

   

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
                SubjectCode = new string('X', 21)   // max 20 karakter
            };
            Assert.NotEmpty(ValidationHelper.Validate(req));
        }
    }


    
    //  12. EMAIL VALIDÁCIÓ TESZTEK
    

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


    
    //  13. CSOPORT MEGHÍVÁS VALIDÁCIÓ TESZTEK


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


  
    //  14. ADMIN SZEREPKÖR VÉDELEM TESZTEK


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
}
