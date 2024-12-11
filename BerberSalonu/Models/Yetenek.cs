using System.Text.Json.Serialization;

namespace BerberSalonu.Models
{
    public class Yetenek
    {
        public int Id {  get; set; }

        public required string Name { get; set; }
        [JsonIgnore]

        public ICollection<Berber> Berberler { get; set; } = new List<Berber>();
    }
}
