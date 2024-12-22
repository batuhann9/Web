using System.Text.Json.Serialization;

namespace BerberSalonu.Models
{
    public class Yetenek
    {
        public int Id {  get; set; }

        public required string Name { get; set; }
        public decimal Price { get; set; }
        public double Sure { get; set; }

        [JsonIgnore]
        public ICollection<BerberYetenek> BerberYetenekler { get; set; } = new List<BerberYetenek>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
