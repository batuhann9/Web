using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.ViewModel
{
    public class BerberYetenekEkleViewModel
    {
        public SelectList? BerberlerSelectList { get; set; }
        public SelectList? YeteneklerSelectList { get; set; }

        [Required(ErrorMessage = "Berber seçimi gereklidir.")]
        public int BerberId { get; set; }

        [Required(ErrorMessage = "Yetenek seçimi gereklidir.")]
        public int YetenekId { get; set; }
    }

}