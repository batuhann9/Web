namespace BerberSalonu.Models
{
    public class Berber
    {
        public int Id { get; set; }
        public required Kullanici Kullanici { get; set; }

        public ICollection<BerberYetenek> BerberYetenekler { get; set; } = new List<BerberYetenek>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

    }
}
