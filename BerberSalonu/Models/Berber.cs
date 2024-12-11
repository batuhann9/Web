namespace BerberSalonu.Models
{
    public class Berber
    {
        public int Id { get; set; }
        public required Kullanici Kullanici { get; set; }

        public ICollection<Yetenek> Yetenekler { get; set; } = new List<Yetenek>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
