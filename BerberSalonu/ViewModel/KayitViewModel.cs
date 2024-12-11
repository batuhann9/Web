using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.ViewModel
{
    public class KayitViewModel
    {
        [Required(ErrorMessage = "Ad alanı gereklidir.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 30 karakter olmalıdır.")]
        [Display(Name = "Ad")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı gereklidir.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 30 karakter olmalıdır.")]
        [Display(Name = "Soyad")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "E-posta alanı gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public required string Password { get; set; }
    }
}
