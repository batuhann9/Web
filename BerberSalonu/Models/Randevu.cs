using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }

        [Required]
        public int BerberId { get; set; }
        public Berber Berber { get; set; }

        [Required]
        public string YetenekId { get; set; }
        public Yetenek Yetenek { get; set; }

        [Required]
        public DateTime RandevuTarihi { get; set; }


    }
}

