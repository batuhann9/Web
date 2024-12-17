using BerberSalonu.Models;
using System.ComponentModel.DataAnnotations;

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
    public int YetenekId { get; set; } // String yerine int olarak düzeltildi
    public Yetenek Yetenek { get; set; }

    [Required]
    public DateTime RandevuTarihi { get; set; }
    public bool IsOnaylandi { get; set; } = false; // Onay durumu (varsayılan: false)
}