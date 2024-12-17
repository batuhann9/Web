namespace BerberSalonu.Models
{
    public class BerberYetenek
    {
        public int Id { get; set; } 
        public int BerberId { get; set; }
        public Berber Berber { get; set; }
        public int YetenekId { get; set; }
        public Yetenek Yetenek { get; set; }
    }
}
