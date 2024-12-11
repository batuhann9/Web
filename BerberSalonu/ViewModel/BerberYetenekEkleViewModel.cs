using BerberSalonu.Models;
using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.ViewModel
{
    public class BerberYetenekEkleViewModel
    {
        public List<Berber> Berberler { get; set; } = new List<Berber>();
        public List<Yetenek> Yetenekler { get; set; } = new List<Yetenek>();

        [Required(ErrorMessage = "Berber seçimi gereklidir.")]
        public int BerberId { get; set; }

        [Required(ErrorMessage = "Yetenek seçimi gereklidir.")]
        public int YetenekId { get; set; }
    }

}