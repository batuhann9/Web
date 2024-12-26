using BerberSalonu.Models;
using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int MusteriId { get; set; }
        public Musteri Musteri { get; set; }

        [Required]
        public int BerberId { get; set; }
        public Berber Berber { get; set; }

        [Required]
        public int YetenekId { get; set; }
        public Yetenek Yetenek { get; set; }

        [Required(ErrorMessage = "Lütfen bir randevu tarihi seçin.")]
        public DateOnly RandevuTarihi { get; set; }

        [Required(ErrorMessage = "Lütfen bir randevu saati seçin.")]
        public TimeOnly RandevuSaati { get; set; }
        
        [Required]
        public RandevuDurum Durum { get; set; } //+2 farklı durum için enum
    }
}