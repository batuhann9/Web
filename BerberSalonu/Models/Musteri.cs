namespace BerberSalonu.Models
{
    public class Musteri
    {
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public required Kullanici Kullanici { get; set; }
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
