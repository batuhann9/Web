using BerberSalonu.Models;

namespace BerberSalonu.vm
{
    public class BerberViewModel
    {
        public required string Name { get; set; }
        public required int Age { get; set; }
        public required ICollection<Yetenek> Yetenekler { get; set; }
        public required string Zaman { get; set; }
    }
}
